using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_movement : MonoBehaviour
{
   [SerializeField] float mouse_scroll_multiplier = 1.0f;
   [SerializeField] float mouse_movement_multiplier = 1.0f;

   [SerializeField] float cam_min_position = 0.0f;
   [SerializeField] float cam_max_position = 10.0f;

   [SerializeField] float cam_min_vertical_rotation = 10.0f;
   [SerializeField] float cam_max_vertical_rotation = 90.0f;

   [SerializeField] Transform cam_pivot;
   [SerializeField] Transform cam;

   // Update is called once per frame
   void Update()
   {
      float scroll_value = Input.mouseScrollDelta.y * mouse_scroll_multiplier;
      Vector2 mouse_movement;
      mouse_movement.x = 0.0f;
      mouse_movement.y = 0.0f;

      if(Input.GetMouseButton(1))
      {
         mouse_movement.x = Input.GetAxis("Mouse X") * mouse_movement_multiplier;
         mouse_movement.y = Input.GetAxis("Mouse Y") * mouse_movement_multiplier;
      }

      Vector3 cam_pivot_rot = cam_pivot.rotation.eulerAngles;

      cam_pivot_rot.y += mouse_movement.x;

      if(cam_pivot_rot.y >= 360.0f)
      {
         cam_pivot_rot.y = cam_pivot_rot.y % 360.0f;
      }

      if(cam_pivot_rot.y < 0.0f)
      {
         cam_pivot_rot.y = (-cam_pivot_rot.y) % 360.0f;
         cam_pivot_rot.y = 360.0f - cam_pivot_rot.y;
      }

      cam_pivot.rotation = Quaternion.Euler(cam_pivot_rot);

      

      cam_pivot_rot = cam_pivot.localRotation.eulerAngles;

      cam_pivot_rot.x -= mouse_movement.y;

      if(cam_pivot_rot.x < cam_min_vertical_rotation)
      {
         cam_pivot_rot.x = cam_min_vertical_rotation;
      }

      if(cam_pivot_rot.x > cam_max_vertical_rotation)
      {
         cam_pivot_rot.x = cam_max_vertical_rotation;
      }

      cam_pivot.localRotation = Quaternion.Euler(cam_pivot_rot);

      

      Vector3 cam_local_position = cam.localPosition;
      
      cam_local_position.z += scroll_value * mouse_scroll_multiplier;

      if(-cam_local_position.z < cam_min_position)
      {
         cam_local_position.z = -cam_min_position;
      }

      if(-cam_local_position.z > cam_max_position)
      {
         cam_local_position.z = -cam_max_position;
      }

      cam.localPosition = cam_local_position;

   }
}
