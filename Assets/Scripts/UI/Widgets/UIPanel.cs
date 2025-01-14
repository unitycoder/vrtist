﻿/* MIT License
 *
 * Copyright (c) 2021 Ubisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using UnityEngine;

namespace VRtist
{
    [ExecuteInEditMode]
    [SelectionBase]
    [RequireComponent(typeof(MeshFilter)),
     RequireComponent(typeof(MeshRenderer)),
     RequireComponent(typeof(BoxCollider))]
    public class UIPanel : UIElement
    {
        public enum BackgroundGeometryStyle { Tube, Flat };

        private static readonly string default_widget_name = "New Panel";
        private static readonly float default_width = 0.4f;
        private static readonly float default_height = 0.6f;
        private static readonly float default_margin = 0.02f;
        private static readonly float default_radius = 0.01f;
        private static readonly float default_thickness = 0.001f;
        private static readonly UIPanel.BackgroundGeometryStyle default_bg_geom_style = UIPanel.BackgroundGeometryStyle.Flat;
        public static readonly string default_material_name = "UIPanel";

        [SpaceHeader("Panel Shape Parmeters", 6, 0.8f, 0.8f, 0.8f)]
        [CentimeterFloat] public float margin = default_margin;
        [CentimeterFloat] public float radius = default_radius;
        [CentimeterFloat] public float thickness = default_thickness;
        public BackgroundGeometryStyle backgroundGeometryStyle = default_bg_geom_style;
        public Material source_material = null;

        [SpaceHeader("Subdivision Parameters", 6, 0.8f, 0.8f, 0.8f)]
        public int circleSubdiv = 8;
        public int nbSubdivPerUnit = 1;
        public int nbSubdivCornerFixed = 3;
        public int nbSubdivCornerPerUnit = 3;

        private void OnValidate()
        {
            const float min_width = 0.01f;
            const float min_height = 0.01f;
            const float min_radius = 0.001f;
            const int min_circleSubdiv = 3;
            const int min_nbSubdivPerUnit = 1;
            const int min_nbSubdivCornerFixed = 1;
            const int min_nbSubdivCornerPerUnit = 1;

            if (width < min_width)
                width = min_width;
            if (height < min_height)
                height = min_height;
            if (radius < min_radius)
                radius = min_radius;
            if (margin > width / 2.0f || margin > height / 2.0f)
                margin = Mathf.Min(width / 2.0f, height / 2.0f);
            if (radius > width / 2.0f || radius > height / 2.0f)
                radius = Mathf.Min(width / 2.0f, height / 2.0f);
            if (margin < radius)
                margin = radius;
            if (circleSubdiv < min_circleSubdiv)
                circleSubdiv = min_circleSubdiv;
            if (nbSubdivPerUnit < min_nbSubdivPerUnit)
                nbSubdivPerUnit = min_nbSubdivPerUnit;
            if (nbSubdivCornerFixed < min_nbSubdivCornerFixed)
                nbSubdivCornerFixed = min_nbSubdivCornerFixed;
            if (nbSubdivCornerPerUnit < min_nbSubdivCornerPerUnit)
                nbSubdivCornerPerUnit = min_nbSubdivCornerPerUnit;

            // Realign button to parent anchor if we change the thickness.
            if (-thickness != relativeLocation.z)
                relativeLocation.z = -thickness;

            NeedsRebuild = true;

            // NOTE: RebuildMesh() cannot be called in OnValidate().
        }

        private void Update()
        {
            if (NeedsRebuild)
            {
                RebuildMesh();
                UpdateLocalPosition();
                UpdateAnchor();
                UpdateChildren();
                ResetColor();
                NeedsRebuild = false;
            }
        }

        public override void ResetColor()
        {
            SetColor(Disabled ? DisabledColor
                  : (Hovered ? HoveredColor
                  : BaseColor));
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 labelPosition = transform.TransformPoint(new Vector3(margin + radius, -margin - radius, 0.0f));
            Vector3 posTopLeft = transform.TransformPoint(new Vector3(margin, -margin, 0));
            Vector3 posTopRight = transform.TransformPoint(new Vector3(width - margin, -margin, 0));
            Vector3 posBottomLeft = transform.TransformPoint(new Vector3(+margin, -height + margin, 0));
            Vector3 posBottomRight = transform.TransformPoint(new Vector3(width - margin, -height + margin, 0));

            Gizmos.color = Color.white;
            Gizmos.DrawLine(posTopLeft, posTopRight);
            Gizmos.DrawLine(posTopRight, posBottomRight);
            Gizmos.DrawLine(posBottomRight, posBottomLeft);
            Gizmos.DrawLine(posBottomLeft, posTopLeft);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPosition, gameObject.name);
#endif
        }

        public override void RebuildMesh()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            Mesh theNewMesh = (backgroundGeometryStyle == BackgroundGeometryStyle.Tube)
                ? UIUtils.BuildRoundedRectTubeEx(
                    width, height, margin, radius,
                    circleSubdiv, nbSubdivPerUnit, nbSubdivCornerFixed, nbSubdivCornerPerUnit)
                : UIUtils.BuildRoundedBoxEx(
                    width, height, margin, thickness,
                    nbSubdivCornerFixed, nbSubdivCornerPerUnit);
            theNewMesh.name = "UIPanel_GeneratedMesh";
            meshFilter.sharedMesh = theNewMesh;

            UpdateColliderDimensions();
        }

        private void UpdateColliderDimensions()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            BoxCollider coll = gameObject.GetComponent<BoxCollider>();
            if (meshFilter != null && coll != null)
            {
                Vector3 initColliderCenter = meshFilter.sharedMesh.bounds.center;
                Vector3 initColliderSize = meshFilter.sharedMesh.bounds.size;
                coll.center = initColliderCenter;
                coll.size = initColliderSize;
            }
        }

        public override void ResetMaterial()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Color prevColor = BaseColor;
                if (meshRenderer.sharedMaterial != null)
                {
                    prevColor = meshRenderer.sharedMaterial.GetColor("_BaseColor");
                }

                Material materialInstance = Instantiate(source_material);
                
                meshRenderer.sharedMaterial = materialInstance;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                //meshRenderer.renderingLayerMask = 2; // "LightLayer 1"

                Material sharedMaterialInstance = meshRenderer.sharedMaterial;
                sharedMaterialInstance.name = "UIPanel_Material_Instance";
                sharedMaterialInstance.SetColor("_BaseColor", prevColor);
            }
        }

        #region ray

        public override void OnRayEnter()
        {
            base.OnRayEnter();
            WidgetBorderHapticFeedback();
        }

        public override void OnRayEnterClicked()
        {
            base.OnRayEnterClicked();
            WidgetBorderHapticFeedback();
        }

        public override void OnRayHover(Ray ray)
        {
            base.OnRayHover(ray);
        }

        public override void OnRayHoverClicked()
        {
            base.OnRayHoverClicked();
        }

        public override void OnRayExit()
        {
            base.OnRayExit();
            WidgetBorderHapticFeedback();
        }

        public override void OnRayExitClicked()
        {
            base.OnRayExitClicked();
            WidgetBorderHapticFeedback();
        }

        public override void OnRayClick()
        {
            base.OnRayClick();
        }

        public override void OnRayReleaseInside()
        {
            base.OnRayReleaseInside();
        }

        public override bool OnRayReleaseOutside()
        {
            return base.OnRayReleaseOutside();
        }

        #endregion

        #region create

        public class CreatePanelParams
        {
            public Transform parent = null;
            public string widgetName = UIPanel.default_widget_name;
            public Vector3 relativeLocation = new Vector3(0, 0, -UIPanel.default_thickness);
            public float width = UIPanel.default_width;
            public float height = UIPanel.default_height;
            public float margin = UIPanel.default_margin;
            public float thickness = UIPanel.default_thickness;
            public float radius = UIPanel.default_radius;
            public UIPanel.BackgroundGeometryStyle backgroundGeometryStyle = UIPanel.default_bg_geom_style;
            public Material material = UIUtils.LoadMaterial(UIPanel.default_material_name);
            public ColorVar color = UIOptions.BackgroundColorVar;
        }


        public static UIPanel Create(CreatePanelParams input)
        {
            GameObject go = new GameObject(input.widgetName);
            go.tag = "UICollider";

            // Find the anchor of the parent if it is a UIElement
            Vector3 parentAnchor = Vector3.zero;
            if (input.parent)
            {
                UIElement elem = input.parent.gameObject.GetComponent<UIElement>();
                if (elem)
                {
                    parentAnchor = elem.Anchor;
                }
            }

            UIPanel uiPanel = go.AddComponent<UIPanel>(); // NOTE: also creates the MeshFilter, MeshRenderer and Collider components
            uiPanel.relativeLocation = input.relativeLocation;
            uiPanel.transform.parent = input.parent;
            uiPanel.transform.localPosition = parentAnchor + input.relativeLocation;
            uiPanel.transform.localRotation = Quaternion.identity;
            uiPanel.transform.localScale = Vector3.one;
            uiPanel.width = input.width;
            uiPanel.height = input.height;
            uiPanel.margin = input.margin;
            uiPanel.radius = input.radius;
            uiPanel.thickness = input.thickness;
            uiPanel.backgroundGeometryStyle = input.backgroundGeometryStyle;
            uiPanel.source_material = input.material;
            uiPanel.baseColor.useConstant = false;
            uiPanel.baseColor.reference = input.color;

            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = 
                    (uiPanel.backgroundGeometryStyle == BackgroundGeometryStyle.Tube)
                        ? UIUtils.BuildRoundedRectTube(input.width, input.height, input.margin, input.radius)
                        : UIUtils.BuildRoundedBox(input.width, input.height, input.margin, input.thickness);

                uiPanel.Anchor = Vector3.zero; // TODO: thickness goes +Z and Anchor stays zero? or thickness goes -Z and Anchor follows the surface?

                BoxCollider coll = go.GetComponent<BoxCollider>();
                if (coll != null)
                {
                    Vector3 initColliderCenter = meshFilter.sharedMesh.bounds.center;
                    Vector3 initColliderSize = meshFilter.sharedMesh.bounds.size;
                    coll.center = initColliderCenter;
                    coll.size = initColliderSize;
                    coll.isTrigger = true;
                }
            }

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null && input.material != null)
            {
                // Clone the material.
                meshRenderer.sharedMaterial = Instantiate(input.material);
                Material sharedMaterial = meshRenderer.sharedMaterial;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.renderingLayerMask = 2; // "LightLayer 1"

                uiPanel.SetColor(input.color.value);
            }

            UIUtils.SetRecursiveLayer(go, "CameraHidden");

            return uiPanel;
        }
        #endregion
    }
}
