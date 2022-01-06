function outFig = overlayHUDCues(inFig, z, x, upHUD, downHUD, rightHUD, leftHUD, ax1)
    
    %Finds the indices for each of the arrays where that HUD cue was on
    %(indices for all non-zero values)
    %1 = up, 2 = down, 3 = right, 4 = left
    upHUDIndOn = find(upHUD);
    downHUDIndOn = find(downHUD);
    rightHUDIndOn = find(rightHUD);
    leftHUDIndOn = find(leftHUD);

    figure(inFig);

    %Getting the intervals of the path to plot
    [upZ, upX, upHUDIndOn] = getNonConsec(z, x, upHUDIndOn);
    [downZ, downX, downHUDIndOn] = getNonConsec(z, x, downHUDIndOn);
    [rightZ, rightX, rightHUDIndOn] = getNonConsec(z, x, rightHUDIndOn);
    [leftZ, leftX, leftHUDIndOn] = getNonConsec(z, x, leftHUDIndOn);

    %Colours
    upColour = [255, 79, 79]./256; %Sockeye salmon
    downColour = [168, 0, 0]./256; %Maroon
    rightColour = [5, 0, 120]./256; %Navy
    leftColour = [81, 75, 227]./256; %Soft dark blue

    %Left and Up cues go above the position line, right and down cues go below
    %second = false;
    offSet = 0.04; %+offset = below the graphline, -offset = above the graphline

    up = plot(upZ(upHUDIndOn), -(upX(upHUDIndOn)-2*offSet), 'LineWidth',1.05, 'Color', upColour);
    down = plot(downZ(downHUDIndOn), -(downX(downHUDIndOn)+2*offSet), 'LineWidth',1.05, 'Color', downColour);
    right = plot(rightZ(rightHUDIndOn), -(rightX(rightHUDIndOn)+offSet), 'LineWidth',1.05, 'Color', rightColour);
    left = plot(leftZ(leftHUDIndOn), -(leftX(leftHUDIndOn)-offSet), 'LineWidth',1.05, 'Color', leftColour);

    %Labels for the legend
    %All of them are 12 characters long so that a string array can be used
    legendNames = ['Up Cue On   '; 'Down Cue On '; 'Right Cue On'; 'Left Cue On '];

    %Takes out the labels if that directional cue was never activated in the run
    if ~isempty(upHUDIndOn)
        %Finds and NaNs the corresponding legened label
        legendNames(strcmp(legendNames, 'Up Cue On')) = NaN;
    end
    
    if ~isempty(downHUDIndOn)
        legendNames(strcmp(legendNames, 'Down Cue On')) = NaN;
    end

    if ~isempty(rightHUDIndOn)
        legendNames(strcmp(legendNames, 'Right Cue On')) = NaN;
    end

    if ~isempty(leftHUDIndOn)
        legendNames(strcmp(legendNames, 'Left Cue On')) = NaN;
    end

    %Gets rid of all NaN entries
    %legendNames = rmmissing(legendNames);

    %Copies the axis of the original/main  graph to make a second set of
    %axes so that the second legend can be made
    ax2 = copyobj( ax1, gcf);
    legend(ax2, [up down right left], legendNames, 'Location', [0.55 0.08 0.02 0.1], 'NumColumns', 2);
    
    outFig = inFig;
    
    return;
   
end

function [z, x, HUDIndOn] = getNonConsec(z, x, HUDIndOn)
    
    %Finding non-consecutive indices
    nonConsecUp=find(diff(HUDIndOn) ~= 1) + 1;


    %For each non-consecutive index of z/x (meaning that the HUD cue was off in the interval between them,
    %a NaN is inserted into the next slot of both z and x to make a break in the graph. An extra index is also inserted after 
    %this point in the HUDIndOn array so that the NaN is plotted
    %Ex. if HUDIndOn has 165, 166, 319, 320, then after this function it'll be 165, 166, 167, 319, 320
    %while correspondingly in the x and z arrays a NaN will be inserted into index 167
    
    for n = 1:length(nonConsecUp)
        %nonConsecUp(n)
        HUDIndOn = [HUDIndOn(1:nonConsecUp(n)-1);HUDIndOn(nonConsecUp(n)-1)+1; HUDIndOn(nonConsecUp(n):end)];
        %upHUDIndOn(1:nonConsecUp(n)-1)
        x = [x(1:HUDIndOn(nonConsecUp(n)-1)); NaN; x(HUDIndOn(nonConsecUp(n)):end)];
        z = [z(1:HUDIndOn(nonConsecUp(n)-1)); NaN; z(HUDIndOn(nonConsecUp(n)):end)];
        if n<length(nonConsecUp)
            nonConsecUp(n+1) = nonConsecUp(n+1)+n;
        end
    end

    return;
end