DATA = readmatrix("nazwapliku.csv");
time = DATA(:, 1);
q1 = DATA(:, 2);
q2 = DATA(:, 3);
q3 = DATA(:, 4);
q4 = DATA(:, 5);
Q = DATA(:,2:1:5);
positions = zeros(size(Q,1), 3);

for c = 1:1:size(Q,1)
    [pos] = forward_kinematics(Q(c, :));
    positions(c, :) = pos(5, :);
end

plot_lin_pos_vel(time, positions, "pozycja", "prędkość");
plot_ang_pos_vel(time, q1, "kąt q1", "prędkość kątowa q1");
plot_ang_pos_vel(time, q2, "kąt q2", "prędkość kątowa q2");
plot_ang_pos_vel(time, q3, "kąt q3", "prędkość kątowa q3");
plot_ang_pos_vel(time, q4, "kąt q4", "prędkość kątowa q4");


