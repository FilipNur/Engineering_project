function [] = plot_lin_pos_vel(time, positions, title_pos, title_vel)
    % x
    figure();
    plot(time, positions(:,1));
    grid on;
    title(title_pos + " (X axis)");
    xlabel("czas [s]");
    ylabel("pozycja na osi OX [mm]");

    % y
    figure();
    plot(time, positions(:,2));
    grid on;
    title(title_pos + " (Y axis)");
    xlabel("czas [s]");
    ylabel("pozycja na osi OY [mm]");

    % z
    figure();
    plot(time, positions(:,3));
    grid on;
    title(title_pos + " (Z axis)");
    xlabel("czas [s]");
    ylabel("pozycja na osi OZ [mm]");

    % 3D
    figure();
    plot3(positions(:,3), positions(:,1), positions(:,2), "b");
    grid on;
    hold on;
    plot3(positions(1,3), positions(1,1), positions(1,2), "go", "MarkerSize", 10);
    plot3(positions(size(positions,1),3), positions(size(positions,1),1), positions(size(positions,1),2), "rx", "MarkerSize", 10);
    title(title_pos);
    xlabel("pozycja na osi OZ [mm]");
    ylabel("pozycja na osi OX [mm]");
    zlabel("pozycja na osi OY [mm]");

    % prędkość
    velocity = zeros(size(positions,1),1);
    for c = 1:1:(size(positions,1) - 1)
        d = positions(c + 1, :) - positions (c, :);
        l = sqrt(sum((d.*d), "all"));
        velocity(c) = l / (time(c+1) - time(c));
    end

    figure();
    plot(time, velocity);
    title(title_vel);
    grid on;
    xlabel("czas [s]");
    ylabel("prędkość liniowa [mm/s]");
end