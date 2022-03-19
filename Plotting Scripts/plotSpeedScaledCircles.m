function outFig = plotSpeedScaledCircles(z, x, t, dists, sampRate, inFig, colour, intervalSecs)
        %keyboard
        distsNonZero = dists;
        for n = 2:length(distsNonZero)
            if distsNonZero(n)== 0
                distsNonZero(n) = distsNonZero(n-1);
            end
        end
        
        %Specifies every how many elements from the position data is sampled for the graph according to 
        % how many seconds between each showing of the speed-bubble
        everyNthElem = intervalSecs*sampRate;

        %Speeds
        distsNonZeroSpeeds = distsNonZero.*sampRate;
        
        maxSpeed = max(distsNonZeroSpeeds);
        minSpeed = min(distsNonZeroSpeeds);
        speedRange = max(distsNonZeroSpeeds) - min(distsNonZeroSpeeds);

        figure(inFig);

%|||||||||||||||||||||||||||||||||||COLOUR ONLY|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        %How much faster the speed at the current frame is than the minimum speed as a decimal 
        % (current speed minus the minSpeed divided by the range of speeds)
%         speed2Colour = abs(log10((distsNonZeroSpeeds-minSpeed)/speedRange)); %log base 20
%         colourBySpeed = zeros(length(speed2Colour), 3);
% 
%         for n = 1:length(speed2Colour)
%             colourBySpeed(n,:) = [1-speed2Colour(n), 0, speed2Colour(n)]; %Colour more blue when fast, more red when slow
%         end
% 
%         scatter(z(2:end), -x(2:end), 12, colourBySpeed, 'filled');
%|||||||||||||||||||||||||||||||||||COLOUR ONLY|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


%|||||||||||||||||||||||||||||||||||SIZE ONLY|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        %Vary 10 to 40 for diameter
%         speed2Diameter = 10+30*abs(log10((distsNonZeroSpeeds-minSpeed)/speedRange)); %log base 20
%         speed2Opacity = abs(1-log10((distsNonZeroSpeeds-minSpeed)/speedRange));
% 
%         lengthZ = length(z)
%         lengthDia = length(speed2Diameter)
%         scatter(z(2:end), -x(2:end), speed2Diameter, [217/255 140/255 179/255], 'filled', 'MarkerFaceAlpha',  'flat', 'AlphaData', speed2Opacity);
%|||||||||||||||||||||||||||||||||||SIZE ONLY|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


%|||||||||||||||||||||||||||||||||||SIZE AND COLOUR|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        speed2Diameter = 10+30*abs(log10((distsNonZeroSpeeds-minSpeed)/speedRange));
        speed2Colour = abs(log10((distsNonZeroSpeeds-minSpeed)/speedRange)); %log base 20
        colourBySpeed = zeros(length(speed2Colour), 3);

        for n = 1:length(speed2Colour)
            colourBySpeed(n,:) = [1-speed2Colour(n), 0, speed2Colour(n)]; %Colour more blue when fast, more red when slow
        end

        scatter(z(2:end), -x(2:end), speed2Diameter, colourBySpeed, 'filled');
%|||||||||||||||||||||||||||||||||||SIZE AND COLOUR|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        outFig = inFig;

        return;
end