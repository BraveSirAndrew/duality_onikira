using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

using Duality.Editor;
using Duality.Resources;

namespace Duality.Drawing
{
	public class DrawDevice : IDrawDevice, IDisposable
	{
		/// <summary>
		/// The default reference distance for perspective rendering.
		/// </summary>
		public const float DefaultFocusDist	= 500.0f;

		
		private	bool				disposed		= false;
		private	float				nearZ			= 0.0f;
		private	float				farZ			= 10000.0f;
		private	float				zSortAccuracy	= 0.0f;
		private	float				focusDist		= DefaultFocusDist;
		private	Rect				viewportRect	= Rect.Empty;
		private	Vector3				refPos			= Vector3.Zero;
		private	float				refAngle		= 0.0f;
		private	RenderMatrix		renderMode		= RenderMatrix.OrthoScreen;
		private	PerspectiveMode		perspective		= PerspectiveMode.Parallax;
		private	Matrix4				matModelView	= Matrix4.Identity;
		private	Matrix4				matProjection	= Matrix4.Identity;
		private	Matrix4				matFinal		= Matrix4.Identity;
		private	VisibilityFlag		visibilityMask	= VisibilityFlag.All;
		private	int					pickingIndex	= 0;
		private	List<IDrawBatch>	drawBuffer		= new List<IDrawBatch>();
		private	List<IDrawBatch>	drawBufferZSort	= new List<IDrawBatch>();
		private	int					numRawBatches	= 0;
		private	ContentRef<RenderTarget> renderTarget = null;
		private	uint				hndlPrimaryVBO		= 0;
		private Vector2				nominalViewportSize = new Vector2(1280, 720);


		public bool Disposed
		{
			get { return this.disposed; }
		}
		public Vector3 RefCoord
		{
			get { return this.refPos; }
			set { this.refPos = value; }
		}
		public float RefAngle
		{
			get { return this.refAngle; }
			set { this.refAngle = value; }
		}
		public float FocusDist
		{
			get { return this.focusDist; }
			set { this.focusDist = value; }
		}
		public VisibilityFlag VisibilityMask
		{
			get { return this.visibilityMask; }
			set { this.visibilityMask = value; }
		}
		public float NearZ
		{
			get { return this.nearZ; }
			set
			{
				if (this.nearZ != value)
				{
					this.nearZ = value;
					this.UpdateZSortAccuracy();
				}
			}
		}
		public float FarZ
		{
			get { return this.farZ; }
			set
			{
				if (this.farZ != value)
				{
					this.farZ = value;
					this.UpdateZSortAccuracy();
				}
			}
		}
		/// <summary>
		/// [GET / SET] Specified the perspective effect that is applied when rendering the world.
		/// </summary>
		public PerspectiveMode Perspective
		{
			get { return this.perspective; }
			set { this.perspective = value; }
		}
		public ContentRef<RenderTarget> Target
		{
			get { return this.renderTarget; }
			set { this.renderTarget = value; }
		}
		public int PickingIndex
		{
			get { return this.pickingIndex; }
			set { this.pickingIndex = value; }
		}
		public bool IsPicking
		{
			get { return this.pickingIndex != 0; }
		}
		public RenderMatrix RenderMode
		{
			get { return this.renderMode; }
			set { this.renderMode = value; }
		}
		public Rect ViewportRect
		{
			get { return this.viewportRect; }
			set { this.viewportRect = value; }
		}
		public Vector2 NominalViewportSize
		{
			get { return nominalViewportSize; }
			set { nominalViewportSize = value; }
		}
		/// <summary>
		/// Viewport scaling will scale all rendering to the nominal viewport size, so if the current viewport is 1920x1080
		/// for example, and the nominal viewport size is 1280x720, every rendered object will be scaled by 1.5 times. This
		/// allows games to render to different resolutions without changing the visible area of the game.
		/// </summary>
		public bool UseViewportScaling { get; set; }
		public bool DepthWrite
		{
			get { return this.renderMode != RenderMatrix.OrthoScreen; }
		}
		public Vector2 TargetSize
		{
			get { return this.viewportRect.Size; }
		}

		public Matrix4 MatProjection
		{
			get { return matProjection; }
		}

		public bool IsRenderTargetActive
		{
			get { return this.renderTarget.IsAvailable; }
		}


		public DrawDevice()
		{
			this.UpdateZSortAccuracy();
		}

		~DrawDevice()
		{
			this.Dispose(false);
		}
		/// <summary>
		/// Disposes the DrawDevice.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool manually)
		{
			if (!this.disposed)
			{
				if (DualityApp.ExecContext != DualityApp.ExecutionContext.Terminated &&
					this.hndlPrimaryVBO != 0)
				{
					DualityApp.GuardSingleThreadState();
					GL.DeleteBuffers(1, ref this.hndlPrimaryVBO);
					this.hndlPrimaryVBO = 0;
				}
				this.disposed = true;
			}
		}

		
		/// <summary>
		/// Returns the scale factor of objects that are located at the specified (world space) z-Coordinate.
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public float GetScaleAtZ(float z)
		{
			if (this.perspective == PerspectiveMode.Parallax)
				return this.focusDist / Math.Max(z - this.refPos.Z, this.nearZ);
			else
				return this.focusDist / DefaultFocusDist;
		}
		/// <summary>
		/// Transforms screen space coordinates to world space coordinates. The screen positions Z coordinate is
		/// interpreted as the target world Z coordinate.
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector3 GetSpaceCoord(Vector3 screenPos)
		{
			float targetZ = screenPos.Z;

			// Since screenPos.Z is expected to be a world coordinate, first make that relative
			Vector3 gameObjPos = this.refPos;
			screenPos.Z -= gameObjPos.Z;

			Vector2 targetSize = this.TargetSize;
			screenPos.X -= targetSize.X / 2;
			screenPos.Y -= targetSize.Y / 2;

			MathF.TransformCoord(ref screenPos.X, ref screenPos.Y, this.refAngle);
			
			// Revert active perspective effect
			float scaleTemp;
			if (this.perspective == PerspectiveMode.Flat)
			{
				// Scale globally
				scaleTemp = DefaultFocusDist / this.focusDist;
				screenPos.X *= scaleTemp;
				screenPos.Y *= scaleTemp;
			}
			else if (this.perspective == PerspectiveMode.Parallax)
			{
				// Scale distance-based
				scaleTemp = Math.Max(screenPos.Z, this.nearZ) / this.focusDist;
				screenPos.X *= scaleTemp;
				screenPos.Y *= scaleTemp;
			}
			//else if (this.perspective == PerspectiveMode.Isometric)
			//{
			//    // Scale globally
			//    scaleTemp = DefaultFocusDist / this.focusDist;
			//    screenPos.X *= scaleTemp;
			//    screenPos.Y *= scaleTemp;
				
			//    // Revert isometric projection
			//    screenPos.Z += screenPos.Y;
			//    screenPos.Y -= screenPos.Z;
			//    screenPos.Z += this.focusDist;
			//}
			
			// Make coordinates absolte
			screenPos.X += gameObjPos.X;
			screenPos.Y += gameObjPos.Y;
			screenPos.Z += gameObjPos.Z;

			//// For isometric projection, assure we'll meet the target Z value.
			//if (this.perspective == PerspectiveMode.Isometric)
			//{
			//    screenPos.Y += screenPos.Z - targetZ;
			//    screenPos.Z = targetZ;
			//}

			return screenPos;
		}
		/// <summary>
		/// Transforms screen space coordinates to world space coordinates.
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector3 GetSpaceCoord(Vector2 screenPos)
		{
			return this.GetSpaceCoord(new Vector3(screenPos));
		}
		/// <summary>
		/// Transforms world space coordinates to screen space coordinates.
		/// </summary>
		/// <param name="spacePos"></param>
		/// <returns></returns>
		public Vector3 GetScreenCoord(Vector3 spacePos)
		{
			// Make coordinates relative to the Camera
			Vector3 gameObjPos = this.refPos;
			spacePos.X -= gameObjPos.X;
			spacePos.Y -= gameObjPos.Y;
			spacePos.Z -= gameObjPos.Z;

			// Apply active perspective effect
			float scaleTemp;
			if (this.perspective == PerspectiveMode.Flat)
			{
				// Scale globally
				scaleTemp = this.focusDist / DefaultFocusDist;
				spacePos.X *= scaleTemp;
				spacePos.Y *= scaleTemp;
			}
			else if (this.perspective == PerspectiveMode.Parallax)
			{
				// Scale distance-based
				scaleTemp = this.focusDist / Math.Max(spacePos.Z, this.nearZ);
				spacePos.X *= scaleTemp;
				spacePos.Y *= scaleTemp;
			}

			MathF.TransformCoord(ref spacePos.X, ref spacePos.Y, -this.refAngle);

			Vector2 targetSize = this.TargetSize;
			spacePos.X += targetSize.X / 2;
			spacePos.Y += targetSize.Y / 2;
			
			// Since the result Z value is expected to be a world coordinate, make it absolute
			spacePos.Z += gameObjPos.Z;
			return spacePos;
		}
		/// <summary>
		/// Transforms world space coordinates to screen space coordinates.
		/// </summary>
		/// <param name="spacePos"></param>
		/// <returns></returns>
		public Vector3 GetScreenCoord(Vector2 spacePos)
		{
			return this.GetScreenCoord(new Vector3(spacePos));
		}

		public void PreprocessCoords(ref Vector3 pos, ref float scale)
		{
			if (this.renderMode == RenderMatrix.OrthoScreen) return;
			
			// Make coordinates relative to the Camera
			pos.X -= this.refPos.X;
			pos.Y -= this.refPos.Y;
			pos.Z -= this.refPos.Z;

			// Apply active perspective effect
			float scaleTemp;
			if (this.perspective == PerspectiveMode.Flat)
			{
				// Scale globally
				scaleTemp = this.focusDist / DefaultFocusDist;
				pos.X *= scaleTemp;
				pos.Y *= scaleTemp;
				scale *= scaleTemp;
			}
			else if (this.perspective == PerspectiveMode.Parallax)
			{
				// Scale distance-based
				scaleTemp = this.focusDist / Math.Max(pos.Z, this.nearZ);
				pos.X *= scaleTemp;
				pos.Y *= scaleTemp;
				scale *= scaleTemp;
			}
		}
		public bool IsCoordInView(Vector3 c, float boundRad)
		{
			if (this.renderMode == RenderMatrix.OrthoScreen)
			{
				if (c.Z < this.nearZ) return false;
			}
			else if (c.Z <= this.refPos.Z) return false;

			// Retrieve center vertex coord
			float scaleTemp = 1.0f;
			this.PreprocessCoords(ref c, ref scaleTemp);

			// Apply final (modelview and projection) matrix
			Vector3 oldPosTemp = c;
			Vector3.Transform(ref oldPosTemp, ref this.matFinal, out c);

			// Apply projection matrices XY rotation and scale to bounding radius
			boundRad *= scaleTemp;
			Vector2 boundRadVec = new Vector2(
				boundRad * Math.Abs(this.matFinal.Row0.X) + boundRad * Math.Abs(this.matFinal.Row1.X),
				boundRad * Math.Abs(this.matFinal.Row0.Y) + boundRad * Math.Abs(this.matFinal.Row1.Y));

			return 
				c.Z >= -1.0f &&
				c.Z <= 1.0f &&
				c.X >= -1.0f - boundRadVec.X &&
				c.Y >= -1.0f - boundRadVec.Y &&
				c.X <= 1.0f + boundRadVec.X &&
				c.Y <= 1.0f + boundRadVec.Y;
		}

		public void AddVertices<T>(ContentRef<Material> material, VertexMode vertexMode, params T[] vertices) where T : struct, IVertexData
		{
			this.AddVertices<T>(material.IsAvailable ? material.Res.InfoDirect : Material.Checkerboard.Res.InfoDirect, vertexMode, vertices, vertices.Length);
		}
		public void AddVertices<T>(BatchInfo material, VertexMode vertexMode, params T[] vertices) where T : struct, IVertexData
		{
			this.AddVertices<T>(material, vertexMode, vertices, vertices.Length);
		}
		public void AddVertices<T>(ContentRef<Material> material, VertexMode vertexMode, T[] vertexBuffer, int vertexCount) where T : struct, IVertexData
		{
			this.AddVertices<T>(material.IsAvailable ? material.Res.InfoDirect : Material.Checkerboard.Res.InfoDirect, vertexMode, vertexBuffer, vertexCount);
		}
		public void AddVertices<T>(BatchInfo material, VertexMode vertexMode, T[] vertexBuffer, int vertexCount) where T : struct, IVertexData
		{
			if (vertexCount == 0) return;
			if (vertexBuffer == null || vertexBuffer.Length == 0) return;
			if (vertexCount > vertexBuffer.Length) vertexCount = vertexBuffer.Length;
			if (material == null) material = Material.Checkerboard.Res.InfoDirect;

			if (this.pickingIndex != 0)
			{
				ColorRgba clr = new ColorRgba((this.pickingIndex << 8) | 0xFF);
				for (int i = 0; i < vertexCount; ++i)
					vertexBuffer[i].Color = clr;

				material = new BatchInfo(material);
				material.Technique = DrawTechnique.Picking;
				if (material.Textures == null) material.MainTexture = Texture.White;
			}
			else if (material.Technique == null || !material.Technique.IsAvailable)
			{
				material = new BatchInfo(material);
				material.Technique = DrawTechnique.Solid;
			}
			else if (material.Technique.Res.NeedsPreprocess)
			{
				material = new BatchInfo(material);
				material.Technique.Res.PreprocessBatch<T>(this, material, ref vertexMode, ref vertexBuffer, ref vertexCount);
				if (vertexCount == 0) return;
				if (vertexBuffer == null || vertexBuffer.Length == 0) return;
				if (vertexCount > vertexBuffer.Length) vertexCount = vertexBuffer.Length;
				if (material.Technique == null || !material.Technique.IsAvailable)
					material.Technique = DrawTechnique.Solid;
			}
			
			// When rendering without depth writing, use z sorting everywhere - there's no real depth buffering!
			bool zSort = !this.DepthWrite || material.Technique.Res.NeedsZSort;
			List<IDrawBatch> buffer = zSort ? this.drawBufferZSort : this.drawBuffer;
			float zSortIndex = zSort ? DrawBatch<T>.CalcZSortIndex(vertexBuffer, vertexCount) : 0.0f;

			if (buffer.Count > 0 && buffer[buffer.Count - 1].CanAppendJIT<T>(	
					zSort ? 1.0f / this.zSortAccuracy : 0.0f, 
					zSortIndex, 
					material, 
					vertexMode))
			{
				buffer[buffer.Count - 1].AppendJIT(vertexBuffer, vertexCount);
			}
			else
			{
				buffer.Add(new DrawBatch<T>(material, vertexMode, vertexBuffer, vertexCount, zSortIndex));
			}
			++this.numRawBatches;
		}

		public void AddBatch(IDrawBatch drawBatch)
		{
			if (drawBatch.VertexCount == 0) return;

			BatchInfo material = drawBatch.Material;

			if (material.Technique == null || !material.Technique.IsAvailable)
			{
				material = new BatchInfo(material);
				material.Technique = DrawTechnique.Solid;
			}
			else if (material.Technique.Res.NeedsPreprocess)
			{
				material = new BatchInfo(material);
				material.Technique.Res.PreprocessBatch(this, drawBatch);
				if (material.Technique == null || !material.Technique.IsAvailable)
					material.Technique = DrawTechnique.Solid;
			}

			// When rendering without depth writing, use z sorting everywhere - there's no real depth buffering!
			bool zSort = !this.DepthWrite || material.Technique.Res.NeedsZSort;
			List<IDrawBatch> buffer = zSort ? this.drawBufferZSort : this.drawBuffer;
			drawBatch.CalcZSortIndex();

			if (buffer.Count > 0 && buffer[buffer.Count - 1].CanAppend(drawBatch))
			{
				buffer[buffer.Count - 1].Append(drawBatch);
			}
			else
			{
				buffer.Add(drawBatch);
			}
			++this.numRawBatches;
		}
		
		public void UpdateMatrices()
		{
			Vector2 refSize = this.TargetSize;
			this.GenerateModelView(new Rect(refSize), out this.matModelView);
			this.GenerateProjection(new Rect(refSize), out this.matProjection);
			this.matFinal = this.matModelView * this.MatProjection;
		}
		public void BeginRendering(ClearFlag clearFlags, ColorRgba clearColor, float clearDepth)
		{
			RenderTarget.Bind(this.renderTarget);

			// Setup viewport
			GL.Viewport((int)this.viewportRect.X, (int)this.viewportRect.Y, (int)this.viewportRect.W, (int)this.viewportRect.H);
			GL.Scissor((int)this.viewportRect.X, (int)this.viewportRect.Y, (int)this.viewportRect.W, (int)this.viewportRect.H);

			// Clear buffers
			ClearBufferMask glClearMask = 0;
			if ((clearFlags & ClearFlag.Color) != ClearFlag.None) glClearMask |= ClearBufferMask.ColorBufferBit;
			if ((clearFlags & ClearFlag.Depth) != ClearFlag.None) glClearMask |= ClearBufferMask.DepthBufferBit;
			GL.ClearColor((OpenTK.Graphics.Color4)clearColor);
			GL.ClearDepth((double)clearDepth); // The "float version" is from OpenGL 4.1..
			GL.Clear(glClearMask);

			// Configure Rendering params
			if (this.renderMode == RenderMatrix.OrthoScreen)
			{
				GL.Enable(EnableCap.ScissorTest);
				GL.Enable(EnableCap.DepthTest);
				GL.DepthFunc(DepthFunction.Always);
			}
			else
			{
				GL.Enable(EnableCap.ScissorTest);
				GL.Enable(EnableCap.DepthTest);
				GL.DepthFunc(DepthFunction.Lequal);
			}

			// Upload and adjust matrices
			this.UpdateMatrices();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref this.matModelView);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref this.matProjection);
			if (this.renderTarget.IsAvailable)
			{
				if (this.renderMode == RenderMatrix.OrthoScreen) GL.Translate(0.0f, RenderTarget.BoundRT.Res.Height * 0.5f, 0.0f);
				GL.Scale(1.0f, -1.0f, 1.0f);
				if (this.renderMode == RenderMatrix.OrthoScreen) GL.Translate(0.0f, -RenderTarget.BoundRT.Res.Height * 0.5f, 0.0f);
			}
		}
		public void EndRendering()
		{
			// Process drawcalls
			this.OptimizeBatches();
			this.BeginBatchRendering();

			int drawCalls = 0;
			{
				// Z-Independent: Sorted as needed by batch optimizer
				drawCalls += this.RenderBatches(this.drawBuffer);
				// Z-Sorted: Back to Front
				drawCalls += this.RenderBatches(this.drawBufferZSort);
			}
			Profile.StatNumDrawcalls.Add(drawCalls);

			this.FinishBatchRendering();
			this.drawBuffer.Clear();
			this.drawBufferZSort.Clear();
		}


		private void UpdateZSortAccuracy()
		{
			this.zSortAccuracy = 10000000.0f / Math.Max(1.0f, Math.Abs(this.farZ - this.nearZ));
		}
		private void GenerateModelView(Rect viewport, out Matrix4 mvMat)
		{
			mvMat = Matrix4.Identity;
			if (this.renderMode == RenderMatrix.OrthoScreen) return;

			// Translate objects contrary to the camera
			// Removed: Do this in software now for custom perspective / parallax support
			// modelViewMat *= Matrix4.CreateTranslation(-this.GameObj.Transform.Pos);

			Matrix4 scaleMatrix = Matrix4.Identity;
			if (UseViewportScaling)
			{
				float scaleX = viewport.W / NominalViewportSize.X;
				float scaleY = viewport.H / NominalViewportSize.Y;
				float scale = MathF.Min(scaleX, scaleY);

				float translateX = (viewport.W - (NominalViewportSize.X * scale)) / 2f;
				float translateY = (viewport.H - (NominalViewportSize.Y * scale)) / 2f;

				scaleMatrix = Matrix4.CreateScale(scale, scale, 1) * Matrix4.CreateTranslation(translateX, -translateY, 0);
			}
			// Rotate them according to the camera angle
			mvMat *= scaleMatrix * Matrix4.CreateRotationZ(-this.refAngle);
		}
		private void GenerateProjection(Rect orthoAbs, out Matrix4 projMat)
		{
			if (this.renderMode == RenderMatrix.OrthoScreen)
			{
				Matrix4.CreateOrthographicOffCenter(
					orthoAbs.X,
					orthoAbs.X + orthoAbs.W, 
					orthoAbs.Y + orthoAbs.H, 
					orthoAbs.Y, 
					this.nearZ, 
					this.farZ,
					out projMat);
				// Flip Z direction from "out of the screen" to "into the screen".
				projMat.M33 = -projMat.M33;
			}
			else
			{
				Matrix4.CreateOrthographicOffCenter(
					orthoAbs.X - orthoAbs.W * 0.5f, 
					orthoAbs.X + orthoAbs.W * 0.5f, 
					orthoAbs.Y + orthoAbs.H * 0.5f, 
					orthoAbs.Y - orthoAbs.H * 0.5f, 
					this.nearZ, 
					this.farZ,
					out projMat);
				// Flip Z direction from "out of the screen" to "into the screen".
				projMat.M33 = -projMat.M33;
			}
		}

		private int DrawBatchComparer(IDrawBatch first, IDrawBatch second)
		{
			return first.SortIndex - second.SortIndex;
		}
		private int DrawBatchComparerZSort(IDrawBatch first, IDrawBatch second)
		{
			return MathF.RoundToInt((second.ZSortIndex - first.ZSortIndex) * this.zSortAccuracy);
		}
		private void OptimizeBatches()
		{
			int batchCountBefore = this.drawBuffer.Count + this.drawBufferZSort.Count;
			if (this.pickingIndex == 0) Profile.TimeOptimizeDrawcalls.BeginMeasure();

			// Non-ZSorted
			if (this.drawBuffer.Count > 1)
			{
				this.drawBuffer.StableSort(this.DrawBatchComparer);
				this.drawBuffer = this.OptimizeBatches(this.drawBuffer);
			}

			// Z-Sorted
			if (this.drawBufferZSort.Count > 1)
			{
				// Stable sort assures maintaining draw order for batches of equal ZOrderIndex
				this.drawBufferZSort.StableSort(this.DrawBatchComparerZSort);
				this.drawBufferZSort = this.OptimizeBatches(this.drawBufferZSort);
			}

			if (this.pickingIndex == 0) Profile.TimeOptimizeDrawcalls.EndMeasure();
			int batchCountAfter = this.drawBuffer.Count + this.drawBufferZSort.Count;

			Profile.StatNumRawBatches.Add(this.numRawBatches);
			Profile.StatNumMergedBatches.Add(batchCountBefore);
			Profile.StatNumOptimizedBatches.Add(batchCountAfter);
			this.numRawBatches = 0;
		}
		private List<IDrawBatch> OptimizeBatches(List<IDrawBatch> sortedBuffer)
		{
			List<IDrawBatch> optimized = new List<IDrawBatch>(sortedBuffer.Count);
			IDrawBatch current = sortedBuffer[0];
			IDrawBatch next;
			optimized.Add(current);
			for (int i = 1; i < sortedBuffer.Count; i++)
			{
				next = sortedBuffer[i];

				if (current.CanAppend(next))
				{
					current.Append(next);
				}
				else
				{
					current = next;
					optimized.Add(current);
				}
			}

			return optimized;
		}

		private void BeginBatchRendering()
		{
			if (this.hndlPrimaryVBO == 0) GL.GenBuffers(1, out this.hndlPrimaryVBO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, this.hndlPrimaryVBO);
		}
		private int RenderBatches(List<IDrawBatch> buffer)
		{
			if (this.pickingIndex == 0) Profile.TimeProcessDrawcalls.BeginMeasure();

			int drawCalls = 0;
			List<IDrawBatch> batchesSharingVBO = new List<IDrawBatch>();
			IDrawBatch lastBatchRendered = null;

			IDrawBatch lastBatch = null;
			for (int i = 0; i < buffer.Count; i++)
			{
				IDrawBatch currentBatch = buffer[i];
				IDrawBatch nextBatch = (i < buffer.Count - 1) ? buffer[i + 1] : null;

				if (lastBatch == null || lastBatch.CanShareVBO(currentBatch))
				{
					batchesSharingVBO.Add(currentBatch);
				}

				if (batchesSharingVBO.Count > 0 && (nextBatch == null || !currentBatch.CanShareVBO(nextBatch)))
				{
					int vertexOffset = 0;
					batchesSharingVBO[0].UploadToVBO(batchesSharingVBO);
					drawCalls++;

					foreach (IDrawBatch renderBatch in batchesSharingVBO)
					{
						renderBatch.SetupVBO();
						renderBatch.Render(this, ref vertexOffset, ref lastBatchRendered);
						renderBatch.FinishVBO();
						drawCalls++;
					}

					batchesSharingVBO.Clear();
					lastBatch = null;
				}
				else
					lastBatch = currentBatch;
			}

			if (lastBatchRendered != null)
				lastBatchRendered.FinishRendering();

			if (this.pickingIndex == 0) Profile.TimeProcessDrawcalls.EndMeasure();
			return drawCalls;
		}
		private void FinishBatchRendering()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public static void RenderVoid(Rect viewportRect)
		{
			RenderTarget.Bind(ContentRef<RenderTarget>.Null);
			
			GL.Viewport((int)viewportRect.X, (int)viewportRect.Y, (int)viewportRect.W, (int)viewportRect.H);
			GL.Scissor((int)viewportRect.X, (int)viewportRect.Y, (int)viewportRect.W, (int)viewportRect.H);

			GL.ClearDepth(1.0d);
			GL.ClearColor((OpenTK.Graphics.Color4)ColorRgba.TransparentBlack);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}
	}
}
