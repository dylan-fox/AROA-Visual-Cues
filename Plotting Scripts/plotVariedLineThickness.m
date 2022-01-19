function outFig = plotVariedLineThickness(z, x, t, dists, sampRate, inFig, colour)
        
        distsNonZero = dists;
        for n = 2:length(distsNonZero)
            if distsNonZero(n)== 0
                distsNonZero(n) = distsNonZero(n-1);
            end
        end
        
        distsNonZeroSpeeds = distsNonZero.*sampRate;
        
        maxSpeed = max(distsNonZeroSpeeds);
        minSpeed = min(distsNonZeroSpeeds);
        speedRange = max(distsNonZeroSpeeds) - min(distsNonZeroSpeeds);

        figure(inFig);

        %Line Width 1.25 = normal
        %Range: 0.5 (fastest) to 2.5 (slowest). Median = 2.5
        %Range of line thicknesses is 2

        for n = 2:length(x)-1
%             length(x)
%             n
%How much faster the speed at the current frame is than the minimum speed as a decimal 
% (current speed minus the minSpeed divided by the range of speeds)
            lineThickness = 0.5+2*abs(log10((distsNonZeroSpeeds(n)-minSpeed)/speedRange)); %log base 20
            if lineThickness > 2
                lineThickness = 2;
            end
            plot(z((n-1):n), -x((n-1):n), 'LineWidth', 0.5+lineThickness, 'Color', colour);
        end

        outFig = inFig;

        return;
end