﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TilemapShadowCaster.Runtime
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("Rendering/2D/Tilemap Shadow Caster")]
    public class TilemapShadowCaster2D : MonoBehaviour
    {

        public enum ShadowCasterCreationType {
            [Tooltip("The shadow areas will be created using the colliders outlines. This is efficient and works nice when the lights come from walls. This is the only option that works without self shadows.")]
            Outline,
            [Tooltip("The shadow areas will be created using the colliders mesh. This is less efficient and takes much more time to compute. It works horribly for lights in walls. However it can handle closed spaces.")]
            SingleMesh,
            [Tooltip("The shadow areas will be created using the colliders mesh creating double the objects. This is the least efficient variant, computation takes about the same time as " + nameof(SingleMesh) + ". It works much better for light in walls, however it's still worse than " + nameof(Outline) + ".")]
            DoubleMesh
        }
        [SerializeField] private uint colliderHash;
        [SerializeField] private bool m_SelfShadows = false;
        [SerializeField] private int m_ApplyToSortingLayers = -1;
        [Tooltip("The system by which the shadow paths will be created. Only touch this if you are dealing with lights in closed spaces.")]
        [SerializeField] private ShadowCasterCreationType m_CreationType = ShadowCasterCreationType.Outline;
            
        private void Update()
        {
            ReinitializeShapes(false);
        }
        [ContextMenu("Recompute Shadow Areas")]
        public void InitializeShapes()
            => ReinitializeShapes(true);
        private void ReinitializeShapes(bool force)
        {
            CompositeCollider2D collider = GetComponent<CompositeCollider2D>();
            uint shapeHash = collider.GetShapeHash();
            if (force || shapeHash != colliderHash)
            {
                colliderHash = shapeHash;
                ReinitializeShapes(collider);
            }
        }
        private int[] GetLayers(){
            int[] values = SortingLayer.layers.Select(l => l.id).ToArray();
            List<int> sortingLayers = new List<int>();
            int propCount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                int layer = 1 << i;
                if ((m_ApplyToSortingLayers & layer) != 0)
                {
                    sortingLayers.Add(values[propCount]);
                    propCount ++;
                }
            }
            int[] layerArray = sortingLayers.ToArray();
            return layerArray;
        }

        private void ReinitializeShapes(CompositeCollider2D collider)
        {
            RemoveCurrentShadows();
            
            var ls = GetLayers();
            PathShadow CreatePath(List<Vector2> points) {
                GameObject go = new GameObject("AutogeneratedShadowPath", typeof(MeshRenderer));
                go.transform.parent = transform;
                PathShadow path = go.AddComponent<PathShadow>();
                path.useRendererSilhouette = false;
                path.selfShadows = m_SelfShadows;
                path.SetShape(points, ls);
                return path;
            }
            if(m_CreationType == ShadowCasterCreationType.Outline) {
                for(int i = 0; i < collider.pathCount; i++) {
                    List<Vector2> points = new List<Vector2>();
                    collider.GetPath(i, points);
                    CreatePath(points);
                }
            } else {
                var m = collider.CreateMesh(true, true);
                for(int i = 0; i < m.triangles.Length; i += 3) {
                    CreatePath(new List<Vector2>() {
                        m.vertices[m.triangles[i]],
                        m.vertices[m.triangles[i + 1]],
                        m.vertices[m.triangles[i + 2]] }); 
                    ;
                    if(m_CreationType == ShadowCasterCreationType.DoubleMesh) {
                        CreatePath(new List<Vector2>() {
                            m.vertices[m.triangles[i + 2]],
                            m.vertices[m.triangles[i + 1]],
                            m.vertices[m.triangles[i]] });
                    }
                }
            }
        }

        private void RemoveCurrentShadows()
        {
            new List<PathShadow>(GetComponentsInChildren<PathShadow>())
                .ConvertAll(comp => comp.transform.gameObject)
                .ForEach(gameObject =>
                {
                    if (Application.isEditor)
                    {
                        DestroyImmediate(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                });
        }

        public void ReinitializeShapes()
        {
            ReinitializeShapes(GetComponent<CompositeCollider2D>());
        }

        public void OnDestroy()
        {
            RemoveCurrentShadows();
        }
    }

}