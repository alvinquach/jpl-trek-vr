﻿using UnityEngine;
using System;

namespace TrekVRApplication {

    [Obsolete("Menu interaction is now done using an embedded browser. Use XRBrowser instead.")]
    public class XRMenuPlanetNavigationButton : XRMenuElement {

        [SerializeField]
        private XRInteractableGlobeTerrain _globe;

        public float latitude;

        public float longitude;

        //public override void OnTriggerDoubleClick(PrimaryXRController sender, RaycastHit hit, ClickedEventArgs e) {
        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {

            if (_globe) {
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                _globe.NavigateTo(new Vector2(latitude, longitude), eye.transform.position);
            }

        }

    }

}
