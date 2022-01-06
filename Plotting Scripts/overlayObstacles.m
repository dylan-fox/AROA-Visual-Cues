function outFig = overlayObstacles(inFig, layoutNum)

%inFig is the graph from the main script that's been passed onto this
%helper function
%outFig is the same graph returned with the obstacles graphed on
    
%IMPORTANT THING TO NOTE IS THAT WHILE THE YTICK LABELS ON THE ACTUAL GRAPH
%ARE FLIPPED NEGATIVE/POSITIVE TO REFLECT THAT RIGHT = POSITIVE VALUES, ON
%THE GRAPHING HERE IT'S STILL THE ACTUAL VALUES, SO MOVING AN OBSTACLE
%RECTANGLE UP ON THE GRAPH IS STILL POSITIVE

    locationSetX = 0:1.5:15; %11 locations down the hall
    locationSetY =  -0.9:0.3:0.9;%7 locations across the hall

    midWidth = 0.4;
    highLowWideWidth = 0.3;
    highLowHeight = 1.8;
    wideHeight = highLowHeight/2;

    figure(inFig);

    layoutSpecs = layoutStorage(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight);


    %(1 = low, 2 = mid, 3 = high, 4 = wide)
    for n = 1:4
        if ~isnan(layoutSpecs(n,:)) %Checks if the specific obstacle appeared in the given layout
            r = rectangle('Position', layoutSpecs(n,:),  "FaceColor", [0.5 0.5 0.5 0.6], "EdgeColor", 'none');

            if n == 1 %Low
                %Differentiating outline
                r.EdgeColor = [0 0 0];
                r.LineStyle = ':';
                r.LineWidth = 1.25;
                
                %Text label
                txt = text(layoutSpecs(n,1)+highLowWideWidth/2, 1,'Low');
                txt.Rotation = 60;
                txt.FontSize = 8;
            
            elseif n == 2 %Mid
                 %Text label
                txt = text(layoutSpecs(n,1)+midWidth/2, 1,'Mid');
                txt.Rotation = 60;
                txt.FontSize = 8;
            
            elseif n == 3 %High
                %r.FaceColor = [0.7 0.7 0.7 0.4];
                r.EdgeColor = [0.7 0.7 0.7 1];
                r.LineStyle = '--';
                r.LineWidth = 1.25;

                %Text label
                txt = text(layoutSpecs(n,1)+highLowWideWidth/2, 1,'High');
                txt.Rotation = 60;
                txt.FontSize = 8;
      
            elseif n == 4 %Wide
                %Text label
                txt = text(layoutSpecs(n,1)+highLowWideWidth/2, 1,'Wide');
                txt.Rotation = 60;
                txt.FontSize = 8;
            end
        end
    end

    outFig = inFig;

    return;
end

function layout = layoutStorage(layoutNum, locationSetX, locationSetY, midWidth, highLowWideWidth, highLowHeight, wideHeight)

    switch layoutNum
        
        case 1
            low = [NaN, NaN, NaN, NaN]; %NaNs mean that that obstacle wasn't present in the layout
            mid = [locationSetX(10)-midWidth/2, locationSetY(6)-midWidth/2, midWidth, midWidth];
            high = [locationSetX(4)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            wide = [locationSetX(7)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];

            layout = [low; mid; high; wide]; %4x4 array

            return;
        
        case 2
            low = [locationSetX(3)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            mid = [NaN, NaN, NaN, NaN];
            high = [locationSetX(8)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            wide = [locationSetX(6)-highLowWideWidth/2, locationSetY(4), highLowWideWidth, wideHeight];

            layout = [low; mid; high; wide];
       
        case 3
            low = [locationSetX(2)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            mid = [locationSetX(4)-midWidth/2, locationSetY(4)-midWidth/2, midWidth, midWidth];
            high = [locationSetX(6)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            wide = [NaN, NaN, NaN, NaN];

            layout = [low; mid; high; wide];
        
        case 4
            low = [locationSetX(9)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            mid = [locationSetX(3)-midWidth/2, locationSetY(3)-midWidth/2, midWidth, midWidth];
            high = [locationSetX(7)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            wide = [locationSetX(4)-highLowWideWidth/2, locationSetY(4), highLowWideWidth, wideHeight];

            layout = [low; mid; high; wide];
        
        case 5
            low = [locationSetX(7)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            mid = [locationSetX(9)-midWidth/2, locationSetY(5)-midWidth/2, midWidth, midWidth];
            high = [locationSetX(5)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            wide = [locationSetX(3)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, wideHeight];

            layout = [low; mid; high; wide];
       
        case 6
            low = [locationSetX(8)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            mid = [locationSetX(5)-midWidth/2, locationSetY(2)-midWidth/2, midWidth, midWidth];
            high = [locationSetX(10)-highLowWideWidth/2, locationSetY(1), highLowWideWidth, highLowHeight];
            wide = [locationSetX(3)-highLowWideWidth/2, locationSetY(4), highLowWideWidth, wideHeight];

            layout = [low; mid; high; wide];
        
        otherwise
            error("OopS!!! that layout dOESN'T exist! Please enter a layoutNum 1-6!");
    end
end