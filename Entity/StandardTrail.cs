using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neo.IronLua;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail : PathingEntity {

        private static readonly Logger Logger = Logger.GetLogger<StandardTrail>();

        public override float DrawOrder => float.MaxValue;

        public TextureResourceManager TextureResourceManager { get; }

        internal Vector3[][] _sectionPoints;

        public StandardTrail(IPackState packState, ITrail trail) : base(packState, trail) {
            this.TextureResourceManager = TextureResourceManager.GetTextureResourceManager(trail.ResourceManager);

            Initialize(trail);
        }

        private void Populate(AttributeCollection collection, TextureResourceManager resourceManager) {
            Populate_Guid(collection, resourceManager);

            Populate_Alpha(collection, resourceManager);
            Populate_AnimationSpeed(collection, resourceManager);
            Populate_Tint(collection, resourceManager);
            Populate_TrailScale(collection, resourceManager);
            Populate_FadeNearAndFar(collection, resourceManager);
            Populate_Texture(collection, resourceManager);
            Populate_Cull(collection, resourceManager);
            Populate_MapVisibility(collection, resourceManager);
            Populate_CanFade(collection, resourceManager);
            Populate_IsWall(collection, resourceManager);

            Populate_Behaviors(collection, resourceManager);

            // Editor Specific
            // Populate_EditTag(collection, resourceManager);
        }

        private void Initialize(ITrail trail) {
            Populate(trail.GetAggregatedAttributes(), TextureResourceManager.GetTextureResourceManager(trail.ResourceManager));

            if (trail.TrailSections != null) { 
                var trailSections = new List<Vector3[]>(trail.TrailSections.Count());
                foreach (var trailSection in trail.TrailSections) {
                    trailSections.Add(PostProcessing_DouglasPeucker(trailSection.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z)), _packState.UserResourceStates.Advanced.MapTrailDouglasPeuckerError).ToArray());
                }

                _sectionPoints = trailSections.ToArray();

                BuildBuffers(trail);
            } else {
                _sectionBuffers = Array.Empty<VertexBuffer>();
            }

            this.FadeIn();
        }

        public override void Update(GameTime gameTime) {
            if (_sectionPoints != null) {
                var playerPos = GameService.Gw2Mumble.PlayerCharacter.Position;
                float minDistSq = float.MaxValue;

                for (int s = 0; s < _sectionPoints.Length; s++) {
                    ref Vector3[] section = ref _sectionPoints[s];

                    if (section.Length == 0) continue;

                    // Check distance to first point if section has only one point
                    if (section.Length == 1) {
                        float dSq = Vector3.DistanceSquared(playerPos, section[0]);
                        if (dSq < minDistSq) minDistSq = dSq;
                        continue;
                    }

                    // Check distance to each line segment
                    for (int i = 0; i < section.Length - 1; i++) {
                        float dSq = DistanceSquaredToSegment(playerPos, section[i], section[i + 1]);
                        if (dSq < minDistSq) minDistSq = dSq;
                    }
                }

                this.DistanceToPlayer = minDistSq < float.MaxValue ? (float)Math.Sqrt(minDistSq) : -1;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Returns the squared distance from point <paramref name="p"/> to the line segment defined by <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        private static float DistanceSquaredToSegment(Vector3 p, Vector3 a, Vector3 b) {
            Vector3 ab = b - a;
            float abLenSq = ab.LengthSquared();

            if (abLenSq < float.Epsilon) {
                // Degenerate segment (a == b)
                return Vector3.DistanceSquared(p, a);
            }

            // Project p onto the line defined by a->b, clamped to [0,1]
            float t = MathHelper.Clamp(Vector3.Dot(p - a, ab) / abLenSq, 0f, 1f);

            // Closest point on the segment
            Vector3 closest = a + ab * t;

            return Vector3.DistanceSquared(p, closest);
        }

    }
}
