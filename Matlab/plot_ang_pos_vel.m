function [] = plot_ang_pos_vel(time, q, title_pos, title_vel)
    figure();
    plot(time, q);
    title(title_pos);
    grid on;
    xlabel("czas [s]");
    ylabel("pozycja [stopnie]");
    
    q_w = zeros(size(q,1), 1);
    for c = 1:1:(size(q,1) - 1)
        [q_w(c)] = min_angle_distance(q(c), q(c+1));
        q_w(c) = q_w(c) / (time(c+1) - time(c));
    end
    
    q_w(size(q,1)) = 0;
    
    figure();
    plot(time, q_w);
    title(title_vel);
    grid on;
    xlabel("czas [s]");
    ylabel("prędkość kątowa [stopnie/s]");
end