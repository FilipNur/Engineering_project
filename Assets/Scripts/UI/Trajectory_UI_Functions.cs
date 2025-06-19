using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class Trajectory_UI_Functions : MonoBehaviour
{   
    
   [SerializeField] Slider _slider;
   [SerializeField] TextMeshProUGUI _text;
   [SerializeField] TextMeshProUGUI warning_text;
   
   [SerializeField] Toggle _angle_precision_toggle;
   [SerializeField] Toggle _show_UI_toggle;

   [SerializeField] Toggle _show_outer_limit;
   [SerializeField] Toggle _show_inner_limit;

   [SerializeField] GameObject outer_limit;
   [SerializeField] GameObject inner_limit;

   [SerializeField] TMP_InputField _linear_speed_input;
   [SerializeField] TMP_InputField _linear_acceleration_input;
   [SerializeField] TMP_InputField _angular_speed_input;
   [SerializeField] TMP_InputField _angular_acceleration_input;

   [SerializeField] TMP_Dropdown _dropdown_interpolation_change;
   [SerializeField] TMP_Dropdown _dropdown_UI_change;

   [SerializeField] GameObject _dropdown_UI_change_gameobject;
   [SerializeField] GameObject _execution_UI_elements;
   [SerializeField] GameObject _waypoints_UI_elements;
   
   [SerializeField] Toggle _toggle_show_waypoints;
   [SerializeField] Material[] waypoint_material;
   [SerializeField] Transform[] waypoint_transform;

   [SerializeField] Arm_control AC;

   static bool trajectory_calculated = false;

   static List<float> time_final = new List<float>();
   static List<Configuration> configurations_final = new List<Configuration>();

   private static bool run_trajectory = false;
   private static bool run_trajectory_prev = false;

   private static bool trajectory_execution = false;
   private static bool trajectory_stopped = false;

   static int window = 0;



   [SerializeField] TMP_InputField[] pos_x;
   [SerializeField] TMP_InputField[] pos_y;
   [SerializeField] TMP_InputField[] pos_z;
   [SerializeField] TMP_InputField[] q4p_angle;
   [SerializeField] TMP_InputField[] grabber_claw_distance;
   [SerializeField] TMP_InputField[] q1_at_0;
   [SerializeField] Toggle[] is_linear_interpolation_waypoint;
   [SerializeField] Toggle[] is_point_active;

   [SerializeField] Button button_calculate_trajectory;
   [SerializeField] Button button_run_trajectory;
   [SerializeField] Button button_stop_trajectory;
   [SerializeField] Button button_continue_trajectory;


   static float[] pos_x_val;
   static float[] pos_y_val;
   static float[] pos_z_val;
   static float[] q4p_angle_val;
   static float[] grabber_claw_distance_val;
   static float[] q1_at_0_val;


   static int prev_index;
   static float elapsed_time;

   static float linear_speed = 100.0f;
   static float linear_acceleration = 30.0f;
   static float angular_speed = 70.0f;
   static float angular_acceleration = 25.0f;

   void Start()
   {
      warning_text.text = "";
      button_calculate_trajectory.interactable = false;
      button_run_trajectory.interactable = false;
      button_stop_trajectory.interactable = false;
      button_continue_trajectory.interactable = false;

      _linear_speed_input.text = linear_speed.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));
      _linear_acceleration_input.text = linear_acceleration.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));
      _angular_speed_input.text = angular_speed.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));
      _angular_acceleration_input.text = angular_acceleration.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));

      pos_x_val = new float[pos_x.Length];
      pos_y_val = new float[pos_x.Length];
      pos_z_val = new float[pos_x.Length];
      q4p_angle_val = new float[pos_x.Length];
      grabber_claw_distance_val = new float[pos_x.Length];
      q1_at_0_val = new float[pos_x.Length];

      for(int i = 0; i < pos_x.Length; i++)
      {
         if(i != 0)
         {
            is_linear_interpolation_waypoint[i].isOn = true;
            is_point_active[i].isOn = true;
         }

         pos_x_val[i] = 0.0f; 
         pos_y_val[i] = 0.0f; 
         pos_z_val[i] = 0.0f; 
         q4p_angle_val[i] = 0.0f; 
         grabber_claw_distance_val[i] = 0.0f; 
         q1_at_0_val[i] = 0.0f; 

         pos_x[i].text = 0.0f.ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
         pos_y[i].text = 0.0f.ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
         pos_z[i].text = 0.0f.ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
         q4p_angle[i].text = 0.0f.ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
         grabber_claw_distance[i].text = 0.0f.ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
         q1_at_0[i].text = 0.0f.ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));

      }

      for(int i = 0; i < waypoint_transform.Length; i++)
      {
         waypoint_transform[i].position = Vector3.down * 10.0f;
      }

   }

   void Update()
   {
      inner_limit.SetActive(_show_inner_limit.isOn);
      outer_limit.SetActive(_show_outer_limit.isOn);

      if((trajectory_calculated) && (trajectory_execution == false))
      {
         button_run_trajectory.interactable = true;
      }

      if(trajectory_execution)
      {
         button_stop_trajectory.interactable = true;
         button_calculate_trajectory.interactable = false;
         button_run_trajectory.interactable = false;
         button_continue_trajectory.interactable = false;
      }

      if(trajectory_execution == false)
      {
         button_calculate_trajectory.interactable = true;
         button_stop_trajectory.interactable = false;
      }

      if(trajectory_stopped == false)
      {
         button_continue_trajectory.interactable = false;
      }

      if((trajectory_stopped) && (trajectory_calculated))
      {
         button_run_trajectory.interactable = true;
         button_continue_trajectory.interactable = true;
         button_continue_trajectory.interactable = true;
      }


      if(_show_UI_toggle.isOn)
      {
         _dropdown_UI_change_gameobject.SetActive(true);
         switch(window)
         {
            case(0):
            {
               _execution_UI_elements.SetActive(true);
               _waypoints_UI_elements.SetActive(false);
            }
            break;

            case(1):
            {
               _execution_UI_elements.SetActive(false);
               _waypoints_UI_elements.SetActive(true);
            }
            break;
         }
         
      }
      else
      {
         _execution_UI_elements.SetActive(false);
         _waypoints_UI_elements.SetActive(false);
         _dropdown_UI_change_gameobject.SetActive(false);
      }


      if(_toggle_show_waypoints.isOn)
      {
         Vector3 temp_vec = new Vector3();

         temp_vec.z = pos_x_val[0];
         temp_vec.y = pos_y_val[0];
         temp_vec.x = pos_z_val[0];

         waypoint_transform[0].localPosition = temp_vec;

         int active_waypoint_count = 0;

         for(int i = 1; i < is_point_active.Length; i++)
         {
            if(is_point_active[i].isOn)
            {
               active_waypoint_count++;
            }
         }

         if(active_waypoint_count != 0)
         {
            int waypoint_index = 1;

            float color_step = 255.0f / (float)active_waypoint_count;

            Color temp_color = new Color();
            temp_color.b = 0.0f;
            temp_color.a = 255.0f;

            for(int i = 1; i < is_point_active.Length; i++)
            {
               if(is_point_active[i].isOn)
               {
                  temp_vec.z = pos_x_val[i];
                  temp_vec.y = pos_y_val[i];
                  temp_vec.x = pos_z_val[i];

                  waypoint_transform[waypoint_index].localPosition = temp_vec;

                  temp_color.r = waypoint_index * color_step;
                  temp_color.g = 255.0f - temp_color.r;

                  waypoint_material[waypoint_index].SetColor("_Color", temp_color/255.0f);

                  waypoint_index++;
               }
               else
               {
                  waypoint_transform[i].position = Vector3.down * 10.0f;
               }
            }
         }
         
      }
      else
      {
         for(int i = 0; i < waypoint_transform.Length; i++)
         {
            waypoint_transform[i].position = Vector3.down * 10.0f;
         }
      }


      if(trajectory_calculated)
      {
         if(time_final.Count > 0)
         {
            _slider.maxValue = time_final.Count - 1;

            if(trajectory_execution == false)
            {
               _text.text = "t = " + time_final[(int)_slider.value].ToString() + "s";
               Configuration temp = configurations_final[(int)_slider.value];
               int set_pos_ret = AC.Set_position(ref temp);

               if(set_pos_ret < 0)
               {
                  warning_text.text = "Angle out of reach on servo " + (-set_pos_ret).ToString() + "!";
               }
               else
               {
                  warning_text.text = "";
               }
               
            }
         }
      }

      if((run_trajectory_prev == false) && (run_trajectory == true))
      {  
         trajectory_execution = true;
         run_trajectory_prev = run_trajectory;
         run_trajectory = false;
         return;
      }

      if(trajectory_execution)
      {
         for(int i = prev_index; i < time_final.Count; i++)
         {
            if(time_final[i] > elapsed_time)
            {
            prev_index = i - 1;

            if(prev_index < 0)
            {
               prev_index = 0;
            }

            break;
            }
            else if(i == (time_final.Count - 1))
            {
               prev_index = i;
               trajectory_execution = false;
            }
         }
         _slider.value = prev_index;
         _text.text = "t = " + time_final[(int)_slider.value].ToString() + "s";
         Configuration c = configurations_final[prev_index];

         int set_pos_ret = AC.Set_position(ref c);

         if(set_pos_ret < 0)
         {
            warning_text.text = "Angle out of reach on servo " + (-set_pos_ret).ToString() + "!";
            trajectory_execution = false;
            run_trajectory_prev = run_trajectory;
            return;
         }

         warning_text.text = "";
         
         elapsed_time += Time.deltaTime;
      }

      run_trajectory_prev = run_trajectory;
   }

   public void Calculate_trajectory_button_clicked()
   {
      warning_text.text = "";

      trajectory_calculated = false;
      trajectory_execution = false;
      trajectory_stopped = false;
      
      List<Position_define> main_waypoints = new List<Position_define>();

      Position_define temp_position;
      temp_position.position.x = pos_x_val[0];
      temp_position.position.y = pos_y_val[0];
      temp_position.position.z = pos_z_val[0];
      temp_position.q1_angle = q1_at_0_val[0];
      temp_position.grabber_angle = q4p_angle_val[0];
      temp_position.grabber_claw = grabber_claw_distance_val[0];
      main_waypoints.Add(temp_position);

      if((temp_position.position.magnitude > AC.get_outer_limit_radius()) || (temp_position.position.magnitude < AC.get_inner_limit_radius()))
      {
         warning_text.text = "Point 1 out of reach!";
         return;
      }

      List<int> waypoints_indices = new List<int>();
      waypoints_indices.Add(0);

      for(int i = 1; i < pos_x.Length; i++)
      {
         if(is_point_active[i].isOn)
         {
            temp_position.position.x = pos_x_val[i];
            temp_position.position.y = pos_y_val[i];
            temp_position.position.z = pos_z_val[i];
            temp_position.q1_angle = q1_at_0_val[i];
            temp_position.grabber_angle = q4p_angle_val[i];
            temp_position.grabber_claw = grabber_claw_distance_val[i];
            main_waypoints.Add(temp_position);
            temp_position.position -= Vector3.up * AC.get_LS1();

            if((temp_position.position.magnitude > AC.get_outer_limit_radius()) || (temp_position.position.magnitude < AC.get_inner_limit_radius()))
            {
               warning_text.text = "Point " + (i+1).ToString() + " out of rach!";
               return;
            }

            waypoints_indices.Add(i);
         }
      }

      if(main_waypoints.Count == 1)
      {
         warning_text.text = "Trajectory contains only one point!";
         return;
      }


      configurations_final.Clear();
      time_final.Clear();

      Position_define pos1, pos2;
      List<Position_define> intermediate_waypoints = new List<Position_define>();

      List<Configuration> conf_temp = new List<Configuration>();
      List<float> time_temp = new List<float>();

      for(int w = 1; w < main_waypoints.Count; w++)
      {
         pos1.position = main_waypoints[w-1].position;
         pos1.q1_angle = main_waypoints[w-1].q1_angle;
         pos1.grabber_angle = main_waypoints[w-1].grabber_angle;
         pos1.grabber_claw = main_waypoints[w-1].grabber_claw;
         
         pos2.position = main_waypoints[w].position;
         pos2.q1_angle = main_waypoints[w].q1_angle;
         pos2.grabber_angle = main_waypoints[w].grabber_angle;
         pos2.grabber_claw = main_waypoints[w].grabber_claw;

         if(is_linear_interpolation_waypoint[waypoints_indices[w]].isOn)
         {
            intermediate_waypoints.Clear();

            int GW_ret = generate_waypoints(pos1, pos2, ref intermediate_waypoints);

            if(intermediate_waypoints.Count > 2)
            {
               if(intermediate_waypoints.Count == 3)
               {
                  switch(GW_ret)
                  {
                     case(1):
                     {
                        AC.Axis_interpolation(intermediate_waypoints[0], intermediate_waypoints[1], angular_speed, angular_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                        if(time_final.Count > 0)
                        {
                           for(int i = 0; i < time_temp.Count; i++)
                           {
                              time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                           }
                        }

                        configurations_final.AddRange(conf_temp);
                        time_final.AddRange(time_temp);
                        
                        
                        int ret_linear = AC.Linear_interpolation(intermediate_waypoints[1], intermediate_waypoints[2], linear_speed, linear_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                        if(ret_linear < 0)
                        {
                           warning_text.text = "Trajectory goes outside of work area!";
                           return;
                        }

                        if(ret_linear > 0)
                        {
                           warning_text.text = "Trajectory point out of reach!";
                           return;
                        }

                        for(int i = 0; i < time_temp.Count; i++)
                        {
                           time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                        }
                     
                        configurations_final.AddRange(conf_temp);
                        time_final.AddRange(time_temp);
                     }
                     break;

                     case(2):
                     {
                        int ret_linear = AC.Linear_interpolation(intermediate_waypoints[0], intermediate_waypoints[1], linear_speed, linear_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                        if(ret_linear < 0)
                        {
                           warning_text.text = "Trajectory goes outside of work area!";
                           return;
                        }

                        if(ret_linear > 0)
                        {
                           warning_text.text = "Trajectory point out of reach!";
                           return;
                        }

                        if(time_final.Count > 0)
                        {
                           for(int i = 0; i < time_temp.Count; i++)
                           {
                              time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                           }
                        }

                        configurations_final.AddRange(conf_temp);
                        time_final.AddRange(time_temp);

                        AC.Axis_interpolation(intermediate_waypoints[1], intermediate_waypoints[2], angular_speed, angular_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                        for(int i = 0; i < time_temp.Count; i++)
                        {
                           time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                        }

                        configurations_final.AddRange(conf_temp);
                        time_final.AddRange(time_temp);
                        
                        

                     }
                     break;
                  }
               }
               else if(intermediate_waypoints.Count == 4)
               {

                  int ret_linear = AC.Linear_interpolation(intermediate_waypoints[0], intermediate_waypoints[1], linear_speed, linear_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                  if(ret_linear < 0)
                  {
                     warning_text.text = "Trajectory goes outside of work area!";
                     return;
                  }

                  if(ret_linear > 0)
                  {
                     warning_text.text = "Trajectory point out of reach!";
                     return;
                  }

                  if(time_final.Count > 0)
                  {
                     for(int i = 0; i < time_temp.Count; i++)
                     {
                        time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                     }
                  }

                  configurations_final.AddRange(conf_temp);
                  time_final.AddRange(time_temp);

                  AC.Axis_interpolation(intermediate_waypoints[1], intermediate_waypoints[2], angular_speed, angular_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                  for(int i = 0; i < time_temp.Count; i++)
                  {
                     time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                  }

                  configurations_final.AddRange(conf_temp);
                  time_final.AddRange(time_temp);

                  ret_linear = AC.Linear_interpolation(intermediate_waypoints[2], intermediate_waypoints[3], linear_speed, linear_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

                  if(ret_linear < 0)
                  {
                     warning_text.text = "Trajectory goes outside of work area!";
                     return;
                  }

                  if(ret_linear > 0)
                  {
                     warning_text.text = "Trajectory point out of reach!";
                     return;
                  }

                  for(int i = 0; i < time_temp.Count; i++)
                  {
                     time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                  }

                  configurations_final.AddRange(conf_temp);
                  time_final.AddRange(time_temp);
               }
               

            }
            else
            {

               int ret_linear = AC.Linear_interpolation(intermediate_waypoints[0], intermediate_waypoints[1], linear_speed, linear_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);
               
               if(ret_linear < 0)
               {
                  warning_text.text = "Trajectory goes outside of work area!";
                  return;
               }

               if(ret_linear > 0)
               {
                  warning_text.text = "Trajectory point out of reach!";
                  return;
               }

               if(time_final.Count > 0)
               {
                  for(int i = 0; i < time_temp.Count; i++)
                  {
                     time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
                  }
               }

               configurations_final.AddRange(conf_temp);
               time_final.AddRange(time_temp);
            }
         }
         else
         {
            AC.Axis_interpolation(pos1, pos2, angular_speed, angular_acceleration, 0.01f, ref time_temp, ref conf_temp, _angle_precision_toggle.isOn);

            if(time_final.Count > 0)
            {
               for(int i = 0; i < time_temp.Count; i++)
               {
                  time_temp[i] += (time_final[time_final.Count - 1] + 0.01f);
               }
            }

            configurations_final.AddRange(conf_temp);
            time_final.AddRange(time_temp);
         }
      }

      trajectory_calculated = true;
      trajectory_stopped = false;

      Configuration temp_configuration;

      for(int i = 0; i < configurations_final.Count; i++)
      {
         if(configurations_final[i].q1 < 0.0f)
         {
            temp_configuration = configurations_final[i];
            temp_configuration.q1 += 360.0f;
            configurations_final[i] = temp_configuration;
         }
      }

      if(_angle_precision_toggle.isOn)
      {
         Write_to_file.write("output_trajectory_servo_resolution_rounding.csv", ref time_final, ref configurations_final);
      }
      else
      {
         Write_to_file.write("output_trajectory_servo_resolution_ignored.csv", ref time_final, ref configurations_final);
      }
   }   

   
   private int generate_waypoints(Position_define start, Position_define end, ref List <Position_define> waypoints)
   {
      Vector3 temp_vec3;
      float path_length_XZ;

      if(is_path_too_close(start.position, end.position) == false)
      {
         waypoints.Add(start);
         waypoints.Add(end);
         return 0;
      }

      float start_XZ_distance;
      
      temp_vec3 = start.position;
      temp_vec3.y = 0.0f;
      start_XZ_distance = temp_vec3.magnitude;
      path_length_XZ = start_XZ_distance;
      
      temp_vec3 = end.position;
      temp_vec3.y = 0.0f;
      path_length_XZ += temp_vec3.magnitude;

      float path_Y_distance = end.position.y - start.position.y;
      float grabber_angle_distance = end.grabber_angle - start.grabber_angle;

      temp_vec3.x = 0.0f;
      temp_vec3.z = 0.0f;

      if(path_length_XZ != 0.0f)
      {
         temp_vec3.y = start.position.y + (path_Y_distance * (start_XZ_distance / path_length_XZ));
      }
      else
      {
         temp_vec3.y = (start.position.y + end.position.y) / 2.0f;
      }
      
      
      Position_define new_waypoint;

      if((start.position.z == 0.0f) && (start.position.x == 0.0f))
      {
         waypoints.Add(start);

         new_waypoint.position.z = 0.0f;
         new_waypoint.position.x = 0.0f;
         new_waypoint.position.y = start.position.y;
         new_waypoint.grabber_angle = start.grabber_angle;

         if((end.position.z == 0.0f) && (end.position.x == 0.0f))
         {
            new_waypoint.q1_angle = end.q1_angle;
         }
         else
         {
            new_waypoint.q1_angle = (AC.Atan2_rad(end.position.x, end.position.z)  / (float)Math.PI) * 180.0f;
         }
         
         new_waypoint.grabber_claw = end.grabber_claw;
         waypoints.Add(new_waypoint);

         waypoints.Add(end);
         return 1;
      }

      if((end.position.z == 0.0f) && (end.position.x == 0.0f))
      {
         waypoints.Add(start);

         new_waypoint.position.z = 0.0f;
         new_waypoint.position.x = 0.0f;
         new_waypoint.position.y = end.position.y;
         new_waypoint.grabber_angle = end.grabber_angle;
         new_waypoint.q1_angle = (AC.Atan2_rad(start.position.x, start.position.z)  / (float)Math.PI) * 180.0f;
         new_waypoint.grabber_claw = end.grabber_claw;
         waypoints.Add(new_waypoint);

         new_waypoint.q1_angle = end.q1_angle;
         waypoints.Add(new_waypoint);
         return 2;
      }

      waypoints.Add(start);

      new_waypoint.position.x = 0.0f;
      new_waypoint.position.y = (start.position.y + end.position.y) / 2.0f;
      new_waypoint.position.z = 0.0f;
      new_waypoint.grabber_angle = start.grabber_angle + (grabber_angle_distance * (start_XZ_distance / path_length_XZ));
      new_waypoint.q1_angle = (AC.Atan2_rad(start.position.x, start.position.z)  / (float)Math.PI) * 180.0f;
      new_waypoint.grabber_claw = end.grabber_claw;

      waypoints.Add(new_waypoint);

      new_waypoint.q1_angle = (AC.Atan2_rad(end.position.x, end.position.z)  / (float)Math.PI) * 180.0f;
      waypoints.Add(new_waypoint);
      
      waypoints.Add(end);

      return 0;
   }


   public void Run_trajectory()
   {
      if(trajectory_calculated)
      {
         prev_index = 0;
         elapsed_time = 0.0f;
         run_trajectory = true;
         trajectory_stopped = false;
      }
      else
      {
         warning_text.text = "Trajectory is not calculated!";
         return;
      }
   }

   public void Stop_trajectory()
   {
      trajectory_execution = false;
      trajectory_stopped = true;
   }

   public void Continue_trajectory()
   {
      trajectory_execution = true;
      trajectory_stopped = false;
   }



   // https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
   // Joshua
   private bool is_path_too_close(Vector3 start_pos, Vector3 end_pos)
   {
      const float min_distance = 0.1f * 100.0f; // 0.1 in arm space, but arm is scaled down by 100, and positions are given in world space which is not scaled down

      float x = 0.0f;
      float y = 0.0f;
      float x1 = start_pos.z;
      float y1 = start_pos.x;
      float x2 = end_pos.z;
      float y2 = end_pos.x;

      float A = x - x1;
      float B = y - y1;
      float C = x2 - x1;
      float D = y2 - y1;

      float dot = A * C + B * D;
      float len_sq = C * C + D * D;

      float param = -1.0f;

      if(len_sq != 0.0f)
      {
         param = dot / len_sq;
      }

      float xx, yy;

      if(param < 0)
      {
         xx = x1;
         yy = y1;
      }
      else if(param > 1)
      {
         xx = x2;
         yy = y2;
      }
      else
      {
         xx = x1 + param * C;
         yy = y1 + param * D;
      }


      float dx = x - xx;
      float dy = y - yy;

      float dist = (float)Math.Sqrt(dx * dx + dy * dy);


      if(dist < min_distance)
      {
         return true;
      }

      return false;
   }



   public void linear_speed_end_edit()
   {
      float prev_val = linear_speed;
      if(float.TryParse(_linear_speed_input.text, NumberStyles.Float, new CultureInfo("en-US"), out linear_speed) == false)
      {
         linear_speed = prev_val;
      }

      if(linear_speed > 300.0f)
      {
         linear_speed = 300.0f;
      }

      if(linear_speed < 0.01f)
      {
         linear_speed = 0.01f;
      }

      _linear_speed_input.text = linear_speed.ToString("0.00", new CultureInfo("en-US"));
   }

   public void linear_acceleraion_end_edit()
   {
      float prev_val = linear_acceleration;

      if(float.TryParse(_linear_acceleration_input.text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out linear_acceleration) == false)
      {
         linear_acceleration = prev_val;
      }
      
      if(linear_acceleration > 100.0f)
      {
         linear_acceleration = 100.0f;
      }

      if(linear_acceleration < 1.0f)
      {
         linear_acceleration = 1.0f;
      }

      _linear_acceleration_input.text = linear_acceleration.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));
   }

   public void angular_speed_end_edit()
   {
      float prev_val = angular_speed;

      if(float.TryParse(_angular_speed_input.text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out angular_speed) == false)
      {
         angular_speed = prev_val;
      }
      
      if(angular_speed > AC.max_servo_speed)
      {
         angular_speed = AC.max_servo_speed;
      }

      if(angular_speed < 1.0f)
      {
         angular_speed = 1.0f;
      }

      _angular_speed_input.text = angular_speed.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));
   }

   public void angular_acceleration_end_edit()
   {
      float prev_val = angular_acceleration;

      if(float.TryParse(_angular_acceleration_input.text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out angular_acceleration) == false)
      {
         angular_acceleration = prev_val;
      }
      
      if(angular_acceleration > 200.0f)
      {
         angular_acceleration = 200.0f;
      }

      if(angular_acceleration < 1.0f)
      {
         angular_acceleration = 1.0f;
      }

      _angular_acceleration_input.text = angular_acceleration.ToString("0.00", CultureInfo.CreateSpecificCulture("en-GB"));
   }

   public void dropdown_UI_value_change()
   {
      window = _dropdown_UI_change.value;
   }

   #region waypoint_edit_fields

      public void pos_x_end_edit(String val)
      {
         int index = (int)(val[0] - '1');

         float prev_val = pos_x_val[index];

         if(float.TryParse(pos_x[index].text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out pos_x_val[index]) == false)
         {
            pos_x_val[index] = prev_val;
         }

         pos_x[index].text = pos_x_val[index].ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
      }

      public void pos_y_end_edit(String val)
      {
         int index = (int)(val[0] - '1');

         float prev_val = pos_y_val[index];

         if(float.TryParse(pos_y[index].text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out pos_y_val[index]) == false)
         {
            pos_y_val[index] = prev_val;
         }

         if(pos_y_val[index] < 0.0f)
         {
            pos_y_val[index] = 0.0f;
         }

         pos_y[index].text = pos_y_val[index].ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
      }

      public void pos_z_end_edit(String val)
      {
         int index = (int)(val[0] - '1');

         float prev_val = pos_z_val[index];

         if(float.TryParse(pos_z[index].text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out pos_z_val[index]) == false)
         {
            pos_z_val[index] = prev_val;
         }

         pos_z[index].text = pos_z_val[index].ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
      }

      public void tool_angle_end_edit(String val)
      {
         int index = (int)(val[0] - '1');

         float prev_val = q4p_angle_val[index];

         if(float.TryParse(q4p_angle[index].text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out q4p_angle_val[index]) == false)
         {
            q4p_angle_val[index] = prev_val;
         }

         q4p_angle[index].text = q4p_angle_val[index].ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
      }

      public void grabber_claw_distance_end_edit(String val)
      {
         int index = (int)(val[0] - '1');

         float prev_val = grabber_claw_distance_val[index];

         if(float.TryParse(grabber_claw_distance[index].text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out grabber_claw_distance_val[index]) == false)
         {
            grabber_claw_distance_val[index] = prev_val;
         }

         if(grabber_claw_distance_val[index] < 0.0f)   
         {
            grabber_claw_distance_val[index] = 0.0f;
         }

         if(grabber_claw_distance_val[index] > 32.0f)   
         {
            grabber_claw_distance_val[index] = 32.0f;
         }

         grabber_claw_distance[index].text = grabber_claw_distance_val[index].ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
      }

      public void q1_at_0_end_edit(String val)
      {
         int index = (int)(val[0] - '1');

         float prev_val = q1_at_0_val[index];

         if(float.TryParse(q1_at_0[index].text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-GB"), out q1_at_0_val[index]) == false)
         {
            q1_at_0_val[index] = prev_val;
         }

         q1_at_0[index].text = q1_at_0_val[index].ToString("0.000", CultureInfo.CreateSpecificCulture("en-GB"));
      }

   #endregion
}
