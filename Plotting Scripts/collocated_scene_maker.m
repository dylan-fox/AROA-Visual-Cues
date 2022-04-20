function collocated_scene_maker
    clear all; close all;
    
    % specify come camera properties
    
    % location in meters
    cam_x = 0;
    cam_y = 0;
    cam_z = 0;
    
    % horizontal and vertical FOV angles (in deg)
    h_fov = 40;
    v_fov = 30;
    
    % specify location for a virtual object
    obj_x = 0;
    obj_y = 0;
    obj_z = 5;
    
    % get vertex coordinates for unit radius sphere
    [X,Y,Z] = sphere;
    
    % shift sphere out from origin
    X = X + obj_x;
    Y = Y + obj_y;
    Z = Z + obj_z;
    
    
    figure; hold on;
    surf(X-1,Y,Z-1);
    surf(X,Y,Z);
    axis equal; axis off;
    xlabel('x (m)'); ylabel('y (m)'); zlabel('z (m)');
    
    campos([cam_x cam_y cam_z]); % specify the location of the camera ==> needs to be position x y z from the datafiles
    camproj('perspective'); % assert perspective projection
    camva(h_fov); % set camera FOV -- need to figure out how to make this non-square - can just put an occluder
    camup([0 1 0]); % set so that positive y is up
    camtarget([0 0 1]); % tell camera to look straight down the z axis

end 