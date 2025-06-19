function [pos] = forward_kinematics(Q)
    LS1 = 70.1351;
    LS2 = 227.241;
    LS3 = 217.621;
    LS4 = 64.999;
    q1 = deg2rad(Q(1));
    q2 = deg2rad(Q(2));
    q3 = deg2rad(Q(3));
    q4 = deg2rad(Q(4));
    
    T1 = [ cos(q1), 0, sin(q1), 0;
           0      , 1, 0      , 0;
          -sin(q1), 0, cos(q1), 0;
          0       , 0, 0      , 1];
    
    T21 = [1, 0       , 0      , 0  ;
           0,  cos(q2), sin(q2), LS1;
           0, -sin(q2), cos(q2), 0  ;
           0, 0       , 0      , 1   ];
    
    T32 = [1, 0       , 0      , 0  ;
           0,  cos(q3), sin(q3), LS2;
           0, -sin(q3), cos(q3), 0  ;
           0, 0       , 0      , 1   ];
    
    T43 = [1, 0       , 0      , 0  ;
           0,  cos(q4), sin(q4), LS3;
           0, -sin(q4), cos(q4), 0  ;
           0, 0       , 0      , 1   ];
    
    TT4 = [1, 0, 0, 0  ;
           0, 1, 0, LS4;
           0, 0, 1, 0  ;
           0, 0, 0, 1   ];
    
    pos = zeros(5,4);
    
    TPT = T1 * T21 * T32 * T43 * TT4;
    TP4 = T1 * T21 * T32 * T43;
    TP3 = T1 * T21 * T32;
    TP2 = T1 * T21;
    
    S = [0,0,0,1]';
    
    pos(5, :) = (TPT * S)';
    pos(4, :) = (TP4 * S)';
    pos(3, :) = (TP3 * S)';
    pos(2, :) = (TP2 * S)';

    pos = pos(:, 1:1:3);
end