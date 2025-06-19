using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;

public struct Position_define
{
   // position of base of grabber
   public Vector3 position;

   // angle of grabber in given position (0 = horizontal orientation),
   // if angle changes during movement, it will change linearly (no acceleration, no deceleration, constant angular speed)
   public float grabber_angle;

   // angle of Q1 servo, ignored when calculating linear movement or if ((position.x != 0) || (position.z != 0))
   public float q1_angle;

   // distance between grabber claws (how much open grabber is), max is 32(mm)
   // if distance changes during movement, it will change linearly (no acceleration, no deceleration, constant linear speed)
   public float grabber_claw;
}

public struct Configuration
{
   public float q1;
   public float q2;
   public float q3;
   public float q4;
   public float grabber_claw;
}

public class Arm_control : MonoBehaviour
{

   #region transforms

      [SerializeField]
      Transform Q1;
      [SerializeField]
      Transform Q2;
      [SerializeField]
      Transform Q3;
      [SerializeField]
      Transform Q4;
      [SerializeField]
      Transform Grabber_claw_1;
      [SerializeField]
      Transform Grabber_claw_2;
   
   #endregion transforms

   // max degrees per second
   //private const float max_servo_speed = 270.27027027f;

   // max degrees per second
   public float max_servo_speed = 220.0f;

   const float grabber_offset = 64.999f;

   const float LS1 = 70.1351f;
   const float LS2 = 227.241f;
   const float LS3 = 217.621f;
   const float LS4 = grabber_offset;

   public float get_LS1()
   {
      return LS1;
   }


   public const float outer_limit = 509.0f;
   public const float inner_limit = 115.0f;

   public float get_outer_limit_radius()
   {
      return outer_limit;
   }

   public float get_inner_limit_radius()
   {
      return inner_limit;
   }

   const float min_servo_angle = -135.0f;
   const float max_servo_angle = 135.0f;

   const float min_Q4_servo_angle = -135.0f;
   const float max_Q4_servo_angle = 90.0f;

   // based on steps per 360 degrees roration
   private const float servo_angle_precision_deg = 360.0f / 4095.0f;

   // mm/s
   private const float grabber_claw_speed = 25.0f;

   public int Set_position(ref Configuration conf)
   {

      Q1.localRotation = Quaternion.Euler(0.0f, -conf.q1, 0.0f);
      Q2.localRotation = Quaternion.Euler(0.0f, 0.0f, conf.q2);
      Q3.localRotation = Quaternion.Euler(0.0f, 0.0f, conf.q3);
      Q4.localRotation = Quaternion.Euler(0.0f, 0.0f, conf.q4);

      if(conf.grabber_claw < 0.0f)
      {
         conf.grabber_claw = 0.0f;
      }

      if(conf.grabber_claw > 32.0f)
      {
         conf.grabber_claw = 32.0f;
      }

      Vector3 temp;
      temp.x = 0.0f;
      temp.y = 0.0f;
      temp.z = conf.grabber_claw/2.0f;
      //temp.z = grabber_current_open/2.0f;
      Grabber_claw_1.localPosition = temp;
      Grabber_claw_2.localPosition = -temp;

      if((conf.q2 < min_servo_angle) || (conf.q2 > max_servo_angle))
      {
         return -2;
      }

      if((conf.q3 < min_servo_angle) || (conf.q3 > max_servo_angle))
      {
         return -3;
      }

      if((conf.q4 < min_Q4_servo_angle) || (conf.q4 > max_Q4_servo_angle))
      {
         return -4;
      }

      return 0;
   }

   // function returns rotation angle with sign that achieves angle b from angle a along shorter arc
   public float min_angle_distance_deg(float a, float b)
   {
      while(true)
      {
         if(a < 0.0f)
         {
            a += 360.0f;
            continue;
         }
         break;
      }

      while(true)
      {
         if(b < 0.0f)
         {
            b += 360.0f;
            continue;
         }
         break;
      }

      float A1 = b - a;
      float A2 = (b + 360.0f) - a;
      float A3 = (b - 360.0f) - a;

      float A1_abs = Math.Abs(A1);
      float A2_abs = Math.Abs(A2);
      float A3_abs = Math.Abs(A3);

      if(A1_abs < A2_abs)
      {
         if(A1_abs < A3_abs)
         {
            return A1;
         }
         else // A1_abs >= A3_abs
         {
            return A3;
         }
      }
      else // A1_abs >= A2_abs
      {
         if(A1_abs < A3_abs)
         {
            return A2;
         }
         else // A1_abs >= A3_abs
         {
            if(A2_abs < A3_abs)
            {
               return A2;
            }
            else // A2_abs >= A3_abs
            {
               return A3;
            }
         }
      }
   }

   public float min_angle_distance_rad(float a, float b)
   {
      while(true)
      {
         if(a < 0.0f)
         {
            a += (float)(Math.PI * 2.0);
            continue;
         }
         break;
      }

      while(true)
      {
         if(b < 0.0f)
         {
            b += (float)(Math.PI * 2.0);
            continue;
         }
         break;
      }

      float A1 = b - a;
      float A2 = (b + (float)(Math.PI * 2.0)) - a;
      float A3 = (b - (float)(Math.PI * 2.0)) - a;

      float A1_abs = Math.Abs(A1);
      float A2_abs = Math.Abs(A2);
      float A3_abs = Math.Abs(A3);

      if(A1_abs < A2_abs)
      {
         if(A1_abs < A3_abs)
         {
            return A1;
         }
         else // A1_abs >= A3_abs
         {
            return A3;
         }
      }
      else // A1_abs >= A2_abs
      {
         if(A1_abs < A3_abs)
         {
            return A2;
         }
         else // A1_abs >= A3_abs
         {
            if(A2_abs < A3_abs)
            {
               return A2;
            }
            else // A2_abs >= A3_abs
            {
               return A3;
            }
         }
      }
   }

   private void servo_angle_precision(ref Configuration Q)
   {
      Q.q1 = servo_angle_precision(Q.q1);
      Q.q2 = servo_angle_precision(Q.q2);
      Q.q3 = servo_angle_precision(Q.q3);
      Q.q4 = servo_angle_precision(Q.q4);
   }

   // function rounding given angle to the nearest angle that is a multiple of servo positioning precision
   private float servo_angle_precision(float q)
   {  
      bool negative = false;

      if(q < 0)
      {
         negative = true;
         q = -q;
      }

      float o;
      float r = q % servo_angle_precision_deg;

      if(r < (servo_angle_precision_deg / 2.0f))
      {
         o = (q - r);   
      }
      else
      {
         o = (q - r + servo_angle_precision_deg);
      }

      if(negative)
      {
         return -o;
      }

      return o;
   }

   // position argument is position of middle of inner part of grabber
   //
   // from middle of inner part of grabber, there is 64.999mm to the end of 3rd segment of arm,
   // inverse kinematics are calculated to that point, and angle of grabber servo is derived from anted angle of grabber and angle of 3rd segment
   //
   // grabber_angle argument is angle in degrees, from horizontal oriantation of grabber, positive angle means grabber facing up, negative - down,
   // the range of possible grabber_angle will change depending on angle of 3rd segment of arm
   //
   // q1_angle is angle value that will be used for angle q1 if (position.x == 0) and (position.z == 0)
   public int Inverse_kinematics(Vector3 position, float grabber_angle, float q1_angle, ref Configuration out_Q)
   {
      int ret = 0;

      if(((position - Vector3.up * LS1).magnitude > outer_limit) ||
         ((position - Vector3.up * LS1).magnitude < inner_limit))
      {
         ret = -1;
      }

      // q1
      if((position.x == 0.0f) && (position.z == 0.0f))
      {
         out_Q.q1 = (q1_angle / 180.0f) * (float)Math.PI;
      }
      else
      {
         out_Q.q1 = Atan2_rad(position.x, position.z);
      }

      // V_K4

      double q4p = grabber_angle * Math.PI/180.0;
      
      Vector3 V_K4;
      V_K4.x = (float)(LS4 * Math.Cos(q4p) * Math.Sin(out_Q.q1));
      V_K4.y = (float)(LS4 * Math.Sin(q4p));
      V_K4.z = (float)(LS4 * Math.Cos(q4p) * Math.Cos(out_Q.q1));

      Vector3 P_Q4 = position - V_K4;

      Vector3 P_Q4p = P_Q4 - (Vector3.up * LS1);

      float L = P_Q4p.magnitude;

      if(L > (LS2 + LS3))
      {
         return 1;
      }

      double alpha = Math.Acos((LS2*LS2 + LS3*LS3 - L*L)/(2 * LS2 * LS3));
      out_Q.q3 = (float)(alpha - Math.PI);


      float q1p, qQ4p;

      q1p = out_Q.q1;

      float qQ4 = Atan2_rad(P_Q4.x, P_Q4.z);

      qQ4p = qQ4;

      float gamma;
      
      if((Math.Abs(min_angle_distance_rad(q1p, qQ4p)) < (float)(Math.PI / 2.0)) || ((P_Q4.x == 0.0f) && (P_Q4.z == 0.0f)))
      {
         gamma = Atan2_rad(P_Q4p.y, (float)Math.Sqrt(P_Q4p.x*P_Q4p.x + P_Q4p.z*P_Q4p.z));
      }
      else
      {
         gamma = Atan2_rad(P_Q4p.y, (float)-Math.Sqrt(P_Q4p.x*P_Q4p.x + P_Q4p.z*P_Q4p.z));
      }


      float beta = (float)Math.Acos((L*L + LS2*LS2 - LS3*LS3)/(2*L*LS2));

      out_Q.q2 = beta + gamma - (float)(Math.PI/2.0);

      float S3_angle = out_Q.q2 + out_Q.q3;

      out_Q.q4 = (float)(-Math.PI/2.0 - S3_angle + q4p);

      out_Q.q1 = (out_Q.q1 * 180.0f) / (float)Math.PI;
      out_Q.q2 = (out_Q.q2 * 180.0f) / (float)Math.PI;
      out_Q.q3 = (out_Q.q3 * 180.0f) / (float)Math.PI;
      out_Q.q4 = (out_Q.q4 * 180.0f) / (float)Math.PI;

      if((out_Q.q2 > max_servo_angle) || (out_Q.q2 < min_servo_angle) ||
         (out_Q.q3 > max_servo_angle) || (out_Q.q3 < min_servo_angle) ||
         (out_Q.q4 > max_Q4_servo_angle) || (out_Q.q4 < min_Q4_servo_angle))
         {
            ret = 1;
         }

      return ret;
   }


   public void Trajectory_times(float max_x, float max_v, float max_a, ref float t1, ref float t2)
   {
      max_x = Math.Abs(max_x);
      max_v = Math.Abs(max_v);
      max_a = Math.Abs(max_a);

      t1 = max_v / max_a;
      
      float x_t1 = max_a * ((t1*t1)/2);

      if(x_t1 >= (max_x / 2.0f))
      {
         t2 = 0.0f;

         if(x_t1 > (max_x / 2.0f))
         {
            t1 = (float)Math.Sqrt(max_x/max_a);
         }
      }
      else
      {
         t2 = (max_x - (2 * x_t1)) / max_v;
      }
   }


   // returns angle in range <0, 2*PI)
   public float Atan2_rad(float y, float x)
   {
      float angle = (float)Math.Atan2(y, x);

      if(angle < 0.0f)
      {
         angle += (float)(2 * Math.PI);
      }

      return angle;
   }

   // function that generates trajectory that follows line from start to end point
   //
   // start_position - defines parameters at start position
   // end_position - defines parameters at end position
   // max_v/a - defines max values for distance, speed and acceleration of arm along the line (should not be infinity. if negative then sign will be changed)
   public int Linear_interpolation(Position_define start_position, Position_define end_position, float max_v, float max_a, float time_step, ref List<float> time_out, ref List<Configuration> configuration_out, bool round_to_servo_precision)
   {
      int IK_ret;

      time_out.Clear();
      configuration_out.Clear();

      if((start_position.position.x == 0.0f) && (start_position.position.z == 0.0f) && (end_position.position.x == 0.0f) && (end_position.position.z == 0.0f) && (start_position.position.y == end_position.position.y))
      {
         Axis_interpolation(start_position, end_position, max_servo_speed, 20.0f, time_step, ref time_out, ref configuration_out, round_to_servo_precision);
         return 0;
      }

      max_v = Math.Abs(max_v);
      max_a = Math.Abs(max_a);
      time_step = Math.Abs(time_step);

      float max_x = Vector3.Magnitude(start_position.position - end_position.position);
      float t1 = 0.0f;
      float t2 = 0.0f;

      Trajectory_times(max_x, max_v, max_a, ref t1, ref t2);

      Configuration config_temp = new Configuration();

      Vector3 current_pos;
      Vector3 start_end = end_position.position - start_position.position;

      float current_time = 0.0f;
      float current_proportion_time = 0.0f;

      float current_q4p;

      Vector3 P1 = new Vector3();
      Vector3 P2 = new Vector3();
      Vector3 P3 = new Vector3();

      Vector3 prev_pos = new Vector3();

      bool failsafe_1 = true;
      bool failsafe_2 = true;

      float q1_at_0;

      #region grabber_claws

         Configuration conf_temp = new Configuration();
         IK_ret = Inverse_kinematics(start_position.position, start_position.grabber_angle, start_position.q1_angle, ref conf_temp);

         if(IK_ret != 0)
         {
            return IK_ret;
         }
         
         conf_temp.grabber_claw = start_position.grabber_claw;
         configuration_out.Add(conf_temp);
         time_out.Add(0.0f);

         float time_step_movement = grabber_claw_speed * time_step * Math.Sign(end_position.grabber_claw - start_position.grabber_claw);

         int absolute_index = 1;
         while(true)
         {
            conf_temp.grabber_claw = start_position.grabber_claw + (time_step_movement * absolute_index);

            if(time_step_movement < 0.0f)
            {
               if(conf_temp.grabber_claw <= end_position.grabber_claw)
               {
                  conf_temp.grabber_claw = end_position.grabber_claw;
                  configuration_out.Add(conf_temp);
                  time_out.Add(time_step * absolute_index);
                  absolute_index++;
                  break;
               }
            }
            else
            {
               if(conf_temp.grabber_claw >= end_position.grabber_claw)
               {
                  conf_temp.grabber_claw = end_position.grabber_claw;
                  configuration_out.Add(conf_temp);
                  time_out.Add(time_step * absolute_index);
                  absolute_index++;
                  break;
               }
            }

            configuration_out.Add(conf_temp);
            time_out.Add(time_step * absolute_index);

            absolute_index++;
         }

      #endregion

      int index = 0;

      while(failsafe_1)
      {  
         if(index == 0)
         {
            q1_at_0 = start_position.q1_angle;
            current_pos = start_position.position + start_end * (x_t(max_x, max_a, t1, t2, current_proportion_time) / max_x);
         }
         else
         {
            q1_at_0 = end_position.q1_angle;
            current_pos = start_position.position + start_end * (x_t(max_x, max_a, t1, t2, current_proportion_time + time_step) / max_x);
         }  
         

         if(current_proportion_time <= (2 * t1 + t2))
         {
            current_q4p = start_position.grabber_angle + (end_position.grabber_angle - start_position.grabber_angle) * (current_proportion_time / (2 * t1 + t2));
         }
         else
         {
            current_q4p = end_position.grabber_angle;
         }

         Inverse_kinematics(current_pos, current_q4p, q1_at_0, ref config_temp);

         if(round_to_servo_precision)
         {  
            q1_at_0 = servo_angle_precision(q1_at_0);
            servo_angle_precision(ref config_temp);
         }
         
         if(index == 0)
         {
            time_out.Add(time_step * absolute_index);
            config_temp.grabber_claw = end_position.grabber_claw;
            configuration_out.Add(config_temp);
            index++;
            absolute_index++;
            current_proportion_time = x_t_inverse(start_position.position, max_x, max_a, t1, t2, current_pos, start_end);
            current_time += time_step;
            prev_pos = current_pos;
            continue;
         }


         P1 = prev_pos;
         P2 = current_pos;

         while(failsafe_2)
         {

            float q1_1 = configuration_out[absolute_index - 1].q1;
            float q1_2 = config_temp.q1;
            float q1_dist = Math.Abs(min_angle_distance_deg(q1_1, q1_2));
            
            if((q1_dist / time_step > max_servo_speed) ||
            (Math.Abs(config_temp.q2 - configuration_out[absolute_index - 1].q2) / time_step > max_servo_speed) ||
            (Math.Abs(config_temp.q3 - configuration_out[absolute_index - 1].q3) / time_step > max_servo_speed) ||
            (Math.Abs(config_temp.q4 - configuration_out[absolute_index - 1].q4) / time_step > max_servo_speed))
            {
               // new point
               P3 = (P1 + P2) / 2.0f;
               Inverse_kinematics(P3, current_q4p, q1_at_0, ref config_temp);

               if(round_to_servo_precision)
               {
                  servo_angle_precision(ref config_temp);
               }
               
               P2 = P3;
            }
            else
            {
               current_pos = P2;

               break;
            }

         }

         time_out.Add(time_step * absolute_index);
         config_temp.grabber_claw = end_position.grabber_claw;
         configuration_out.Add(config_temp);
         const float time_threshold = 0.0001f;
         if(current_proportion_time >= (2 * t1 + t2 - time_threshold))
         {
            break;
         }

         current_proportion_time = x_t_inverse(start_position.position, max_x, max_a, t1, t2, current_pos, start_end);

         prev_pos = current_pos;

         index++;
         absolute_index++;
      }



      return 0;
   }

   // function that generates trajectory in which all servos start and stop at the same time
   //
   // start_position - defines parameters at start position
   // end_position - defines parameters at end position
   // max_w/e - defines max values for angular speed and angular acceleration of all motors (should not be infinity. if negative then sign will be changed)
   // angular speed will be automatically clamped to upped limit of max servo speed
   public int Axis_interpolation(Position_define start_position, Position_define end_position, float max_w, float max_e, float time_step, ref List<float> time_out, ref List<Configuration> configuration_out, bool round_to_servo_precision)
   {
      time_out.Clear();
      configuration_out.Clear();

      max_w = Math.Abs(max_w);
      max_e = Math.Abs(max_e);
      time_step = Math.Abs(time_step);

      if(max_w > max_servo_speed)
      {
         max_w = max_servo_speed;
      }

      Configuration Q_start = new Configuration();
      Configuration Q_end = new Configuration();

      Inverse_kinematics(start_position.position, start_position.grabber_angle, start_position.q1_angle, ref Q_start);
      Inverse_kinematics(end_position.position, end_position.grabber_angle, end_position.q1_angle, ref Q_end);
      

      if((start_position.position.x == 0.0f) && (start_position.position.z == 0.0f))
      {
         Q_start.q1 = start_position.q1_angle;
      }

      if((end_position.position.x == 0.0f) && (end_position.position.z == 0.0f))
      {
         Q_end.q1 = end_position.q1_angle;
      }

      #region grabber_claws

         Configuration conf_temp = new Configuration();

         conf_temp = Q_start;
         conf_temp.grabber_claw = start_position.grabber_claw;
         configuration_out.Add(conf_temp);
         time_out.Add(0.0f);

         float time_step_movement = grabber_claw_speed * time_step * Math.Sign(end_position.grabber_claw - start_position.grabber_claw);

         int absolute_index = 1;
         while(true)
         {
            conf_temp.grabber_claw = start_position.grabber_claw + (time_step_movement * absolute_index);

            if(time_step_movement < 0.0f)
            {
               if(conf_temp.grabber_claw <= end_position.grabber_claw)
               {
                  conf_temp.grabber_claw = end_position.grabber_claw;
                  configuration_out.Add(conf_temp);
                  time_out.Add(time_step * absolute_index);
                  absolute_index++;
                  break;
               }
            }
            else
            {
               if(conf_temp.grabber_claw >= end_position.grabber_claw)
               {
                  conf_temp.grabber_claw = end_position.grabber_claw;
                  configuration_out.Add(conf_temp);
                  time_out.Add(time_step * absolute_index);
                  absolute_index++;
                  break;
               }
            }

            configuration_out.Add(conf_temp);
            time_out.Add(time_step * absolute_index);

            absolute_index++;
         }

      #endregion

      float[] max_theta_q = new float[4];

      max_theta_q[0] = (min_angle_distance_deg(Q_start.q1, Q_end.q1));
      max_theta_q[1] = Q_end.q2 - Q_start.q2;
      max_theta_q[2] = Q_end.q3 - Q_start.q3;
      max_theta_q[3] = Q_end.q4 - Q_start.q4;

      float[] t1 = new float[4];
      float[] t2 = new float[4];

      float[] total_time = new float[4];

      for(int i = 0; i < 4; i++)
      {
         Trajectory_times(Math.Abs(max_theta_q[i]), max_w, max_e, ref t1[i], ref t2[i]);

         total_time[i] = 2 * t1[i] + t2[i];
      }

      int max_time_index = 0;
      float max_time = total_time[0];

      for(int i = 1; i < 4; i++)
      {
         if(total_time[i] > max_time)
         {
            max_time = total_time[i];
            max_time_index = i;
         }
      }

      for(int i = 0; i < 4; i++)
      {
         if(i == max_time_index)
         {
            continue;
         }

         recalculate_trajectory(Math.Abs(max_theta_q[i]), max_e, max_time, ref t1[i], ref t2[i]);
      }
      
      Configuration config_temp = new Configuration();

      bool failsafe_1 = true;

      int index = 0;

      while(failsafe_1)
      {

         config_temp.q1 = Q_start.q1 + Math.Sign(max_theta_q[0]) * x_t(Math.Abs(max_theta_q[0]), max_e, t1[0], t2[0], time_step * index);
         config_temp.q2 = Q_start.q2 + Math.Sign(max_theta_q[1]) * x_t(Math.Abs(max_theta_q[1]), max_e, t1[1], t2[1], time_step * index);
         config_temp.q3 = Q_start.q3 + Math.Sign(max_theta_q[2]) * x_t(Math.Abs(max_theta_q[2]), max_e, t1[2], t2[2], time_step * index);
         config_temp.q4 = Q_start.q4 + Math.Sign(max_theta_q[3]) * x_t(Math.Abs(max_theta_q[3]), max_e, t1[3], t2[3], time_step * index);
         
         config_temp.grabber_claw = end_position.grabber_claw;

         if(round_to_servo_precision)
         {
            servo_angle_precision(ref config_temp);
         }
         
         time_out.Add(time_step * absolute_index);
         configuration_out.Add(config_temp);

         if(time_step * index > max_time)
         {
            break;
         }

         index++;
         absolute_index++;
      }

      return 0;
   }

   private void recalculate_trajectory(float max_theta, float max_e, float t_max, ref float new_t1, ref float new_t2)
   {
      float theta_t_max_2 = max_e * (t_max * t_max) / 8.0f;

      if(theta_t_max_2 == max_theta / 2.0f)
      {
         new_t1 = t_max / 2.0f;
         new_t2 = 0.0f;

         return;
      }

      new_t1 = (t_max / 2.0f) - (float)Math.Sqrt(max_e * max_e * t_max * t_max - 4.0f * max_e * max_theta) / (2 * max_e);
      new_t2 = t_max - 2 * new_t1;

      return;
   }

   private float x_t(float max_x, float max_a, float t1, float t2, float t)
   {
      float v_t1 = max_a * t1;

      if(t <= 0.0f)
      {
         return 0.0f;
      }
      else if(t >= ((2.0f * t1) + t2))
      {
         return max_x;
      }
      else
      {
         if(t <= t1)
         {
            return (float)(max_a * ((t*t)/2.0f));
         }
         else if(t <= (t1 + t2))
         {
            return (float)((v_t1 * (t - t1)) + (max_a * ((t1*t1)/2.0f)));
         }
         else
         {
            return (float)((max_a * ((t * (t1 + t2)) - ((t*t) / 2.0f) - (t1 * t2) - ((t2*t2) / 2.0f))) + (v_t1 * (t - t1)));
         }
      }
   }
   
   private float x_t_inverse(Vector3 start_pos, float max_x, float max_a, float t1, float t2, Vector3 pos, Vector3 start_end)
   {
      float X_t1 = x_t(max_x, max_a, t1, t2, t1);
      float X_t1_t2 = x_t(max_x, max_a, t1, t2, t1 + t2);

      float X = 0.0f;

      float div = 0.0f;

      if(start_end.x != 0.0f)
      {
         X = Math.Abs((pos.x - start_pos.x) / start_end.x);
         div++;
      }

      float Y = 0.0f;
      if(start_end.y != 0.0f)
      {
         Y = Math.Abs((pos.y - start_pos.y) / start_end.y);
         div++;
      }
      
      float Z = 0.0f;
      if(start_end.z != 0.0f)
      {
         Z = Math.Abs((pos.z - start_pos.z) / start_end.z);
         div++;
      }

      float x = (X + Y + Z) / div;
      x = x * max_x;

      float ret = 0.0f;

      if(x <= 0.0f)
      {
         ret = 0.0f;
      }
      else if((x <= X_t1))
      {
         ret = (float)Math.Sqrt((2.0f * x) / max_a);
      }
      else if(x <= X_t1_t2)
      {
         ret = ((x + max_a * (t1 * t1 /2.0f)) / (max_a * t1));
      }
      else if(x < max_x)
      {
         ret = ((max_a * (2 * t1 + t2) - (float)Math.Sqrt(2 * max_a * max_a * (t1 * t1 + t1 * t2) - 2 * max_a * x)) / max_a);
      }
      else
      {
         ret = (2.0f * t1 + t2);
      }

      return ret;
   }
   
}
