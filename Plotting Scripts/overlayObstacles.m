function outFig = overlayObstacles(inFig, layoutNum, HorV, subjHeightMeters)

%inFig is the graph from the main script that's been passed onto this
%helper function
%outFig is the same graph returned with the obstacles graphed on
    midWidth = 0.4;
    highLowWideWidth = 0.3;
    highLowHeight = 1.8;
    wideHeight = highLowHeight/2;

    locationSetX = 0:1.5:15; %11 locations down the hall
    locationSetY =  [-0.9, 0];%-0.9:0.3:0.9;%7 locations across the hall

    labelHeight = 1;

   layoutSpecs = layoutStorageH(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight);
   

if HorV == 1 %Horizontal z,x plots
%IMPORTANT THING TO NOTE IS THAT WHILE THE YTICK LABELS ON THE ACTUAL GRAPH
%ARE FLIPPED NEGATIVE/POSITIVE TO REFLECT THAT RIGHT = POSITIVE VALUES, ON
%THE GRAPHING HERE IT'S STILL THE ACTUAL VALUES, SO MOVING AN OBSTACLE
%RECTANGLE UP ON THE GRAPH IS STILL POSITIVE

    locationSetX = 0:1.5:15; %11 locations down the hall
    locationSetY =  [-0.9, 0];%-0.9:0.3:0.9;%7 locations across the hall


    layoutSpecs = layoutStorageH(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight);
   

elseif HorV == 2 %Vertical
    locationSetX = 0:1.5:15; %11 locations down the hall
    locationSetY =  [-0, subjHeightMeters+0.15-0.5*highLowWideWidth];%2.5-highLowWideWidth];%-0.9:0.3:0.9;%7 locations across the hall

    highLowHeight = highLowWideWidth;
    wideHeight = 2;
    labelHeight = 2.6;

    wideWidth = 0.05;

    layoutSpecs = layoutStorageV(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight, wideWidth);
end


    figure(inFig);
    %layoutNum = 6;

    for n = 1:6
        if ~isnan(layoutSpecs(n,:)) %Checks if the specific obstacle appeared in the given layout
            r = rectangle('Position', layoutSpecs(n,:),  "FaceColor", [0.5 0.5 0.5 0.6], "EdgeColor", 'none');

            if  n == 3 || n == 4 %Low
                %Differentiating outline
                r.EdgeColor = [0 0 0];
                r.LineStyle = ':';
                r.LineWidth = 1.25;
                
                %Text label
                txt = text(layoutSpecs(n,1)+highLowWideWidth/2, labelHeight,'Low');
                txt.Rotation = 60;
                txt.FontSize = 8;
%             
%             elseif n == 2 %Mid
%                  %Text label
%                 txt = text(layoutSpecs(n,1)+midWidth/2, 1,'Mid');
%                 txt.Rotation = 60;
%                 txt.FontSize = 8;
            
            elseif n == 5 || n == 6 %High
                %r.FaceColor = [0.7 0.7 0.7 0.4];
                r.EdgeColor = [0.7 0.7 0.7 1];
                r.LineStyle = '--';
                r.LineWidth = 1.25;

                %Text label
                txt = text(layoutSpecs(n,1)+highLowWideWidth/2, labelHeight,'High');
                txt.Rotation = 60;
                txt.FontSize = 8;
      
            elseif n == 1 || n == 2 %Wide
                %Text label
                txt = text(layoutSpecs(n,1)+highLowWideWidth/2, labelHeight,'Wide');
                txt.Rotation = 60;
                txt.FontSize = 8;
            end
        end
    end

    outFig = inFig;

    return;
end

function layout = layoutStorageH(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight)

%wide is always either 0.9 or 0
    switch layoutNum
        case 1
            wide1 = [1.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            wide2 = [3-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            low1 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [NaN, NaN, NaN, NaN]; %NaNs mean that that obstacle wasn't present in the layout
            high1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [12-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
        
        case 2
            wide1 = [1.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            wide2 = [4.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            low1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [7.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [12-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
       
        case 3
            wide1 = [3-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            wide2 = [4.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            low1 = [9-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [NaN, NaN, NaN, NaN]; 
            high1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
        
        case 4
            wide1 = [1.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            wide2 = [3-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            low1 = [4.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [7.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
        
        case 5
            wide1 = [1.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            wide2 = [4.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            low1 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [7-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [12-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
       
        case 6
            wide1 = [3-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            wide2 = [4.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            low1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [7.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [NaN, NaN, NaN, NaN];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array

        case 7
            wide1 = [1.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            wide2 = [4.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            low1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [7.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
                
        case 8
            wide1 = [1.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, wideHeight];
            wide2 = [3-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
            low1 = [NaN, NaN, NaN, NaN]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [9-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array

        otherwise
            error("OopS!!! that layout dOESN'T exist! Please enter a layoutNum 1-8!");
    end
end
  
function layout = layoutStorageV(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight, wideWidth)

%wide is always either 0.9 or 0
    switch layoutNum
        case 1
            wide1 = [1.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [3-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [NaN, NaN, NaN, NaN]; %NaNs mean that that obstacle wasn't present in the layout
            high1 = [6-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [12-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
        
        case 2
            wide1 = [1.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [4.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [7.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [12-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
       
        case 3
            wide1 = [3-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [4.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [9-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [NaN, NaN, NaN, NaN]; 
            high1 = [6-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
        
        case 4
            wide1 = [1.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [3-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [4.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [6-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [7.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
        
        case 5
            wide1 = [1.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [4.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [13.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [7-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [12-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
       
        case 6
            wide1 = [3-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [4.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight]; 
            high1 = [7.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [NaN, NaN, NaN, NaN];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array

        case 7
            wide1 = [1.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [4.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [6-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [7.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array
                
        case 8
            wide1 = [1.5-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            wide2 = [3-wideWidth/2, locationSetY(1), wideWidth, wideHeight];
            low1 = [NaN, NaN, NaN, NaN]; 
            low2 = [10.5-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight]; 
            high1 = [9-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];
            high2 = [13.5-highLowWideWidth/2, locationSetY(2), highLowWideWidth, highLowHeight];

            layout = [wide1; wide2; low1; low2; high1; high2]; %6x4 array

        otherwise
            error("OopS!!! that layout dOESN'T exist! Please enter a layoutNum 1-8!");
    end
end

% The old b version of the layout - only one of each type of obstacle, up to 4 obstacles
        % case 1
%             low = [NaN, NaN, NaN, NaN]; %NaNs mean that that obstacle wasn't present in the layout
%             mid = [locationSetX(10)-midWidth/2, locationSetY(6)-midWidth/2, midWidth, midWidth];
%             high = [locationSetX(4)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             wide = [locationSetX(7)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
% 
%             layout = [low; mid; high; wide]; %4x4 array
% 
%             return;
%         
%         case 2
%             low = [locationSetX(3)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             mid = [NaN, NaN, NaN, NaN];
%             high = [locationSetX(8)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             wide = [locationSetX(6)-highLowWideWidth/2, locationSetY(4), highLowWideWidth, wideHeight];
% 
%             layout = [low; mid; high; wide];
%        
%         case 3
%             low = [locationSetX(2)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             mid = [locationSetX(4)-midWidth/2, locationSetY(4)-midWidth/2, midWidth, midWidth];
%             high = [locationSetX(6)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             wide = [NaN, NaN, NaN, NaN];
% 
%             layout = [low; mid; high; wide];
%         
%         case 4
%             low = [locationSetX(9)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             mid = [locationSetX(3)-midWidth/2, locationSetY(3)-midWidth/2, midWidth, midWidth];
%             high = [locationSetX(7)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             wide = [locationSetX(4)-highLowWideWidth/2, locationSetY(4), highLowWideWidth, wideHeight];
% 
%             layout = [low; mid; high; wide];
%         
%         case 5
%             low = [locationSetX(7)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             mid = [locationSetX(9)-midWidth/2, locationSetY(5)-midWidth/2, midWidth, midWidth];
%             high = [locationSetX(5)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             wide = [locationSetX(3)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];
% 
%             layout = [low; mid; high; wide];
%        
%         case 6
%             low = [locationSetX(8)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             mid = [locationSetX(5)-midWidth/2, locationSetY(2)-midWidth/2, midWidth, midWidth];
%             high = [locationSetX(10)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
%             wide = [locationSetX(3)-highLowWideWidth/2, locationSetY(4), highLowWideWidth, wideHeight];
% 
%             layout = [low; mid; high; wide];
