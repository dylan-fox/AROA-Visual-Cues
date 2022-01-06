
%{NOTES: Currently having the script run through everything - may be 
% advantageous to have an option to choose by subj and/or condition. 
% Also, should rename all "trialType" to "trialCondition" 
%}

clear all;
close all;

%Set datapath to the PCA folder

datapath = '../PosPCAData/'; %'./Data/PCA/';

%List of Files
% files = dir(datapath); %Finding the PCA csv files in the directory
% files = files(3:end);

listing = dir(datapath);
listing = listing(3:end); 

sampRate = 50; %Sampling Rate

%For storing the min/max X and Y values from the position data
maxX = 0;
minX = 0;
maxY = 0;
minY = 0;

%Colours
colour1 = [82, 161, 181]./256; %teal
colour2 = [255, 180, 26]./256; %orange
colour3 = [230, 26, 113]./256; %rose
colour4 = [205, 135, 255]./256; %lavender
colour5 = [175, 255, 117]./256; %honeydew

colours = [colour1; colour2; colour3; colour4; colour5];

counter = 1;

for s = 1:length(listing); %goes through all folders

    if listing(s).isdir
        
        dirname = listing(s).name;

        typeID = 1;

        % file list
        files = dir([datapath dirname]);

        if isempty(strfind(dirname,'exclude'))
           
            for f = 1:length(files)
            
                if strfind(files(f).name,'csv')
 
                    %Opens up a figure window
                    fig = figure(counter);
                    ax1 = axes;
                    grid on;
                    hold on;
                    %Set the scale ratio to be 1
                    set(gca,'DataAspectRatio',[1 1 1], 'color', 'none');%'XTick',[], 'YTick', [])
                    %Sets the size of the graph in pixels
                    set(gcf,'Position',[0, 0, 6400, 300]);%maxX*400,maxY*400]);
                    %Resizes the axes within the figure (as a proportion of the total figure size
                    set(gca,'OuterPosition',[0.03, 0, 0.97, 0.97])
                    
                    %ARXX_date
                    sbjFileName = files(f).name(1:13);
                    %No cue, collocated, combined, etc. from the filename
                    trialType = files(f).name(15:end-26);
                    %Layout Number
                    layoutNum = str2double(files(f).name(end-17));
        
                    % read in data from csv, convert from table to array
                    C = table2array(readtable([datapath dirname '/' files(f).name]));
        
                    %Get z, x, t
                    z = C(:,1);
                    x = C(:,2);
                    t = C(:,3);

                    %Get HUD cue binaries 
                    %0 = false, 1 = true
                    upHUD = C(:, 4);
                    rightHUD = C(:, 5);
                    downHUD = C(:, 6);
                    leftHUD = C(:, 7);
                    
                    %Find max and min values for the x and y axes
                    %there's probably a less clunky way to do this
                    if(max(z) > maxX)
                        maxX = max(z);
                    end
                    
                    if(max(x) > maxY)
                        maxY = max(x);
                    end
                    
                    if(min(z) < minX)
                        minX = min(z);
                    end
                    
                    if(min(x) < minY)
                        minY = min(x);
                    end
                    
                    %Get the differences between adjacent elements of the vector
                    zDiffs = diff(z);
                    xDiffs = diff(x);
                    tDiffs = diff(t);
                    
                    %Distance
                    dists = sqrt(xDiffs.^2 + zDiffs.^2);

                    %The cumuSum and speed calcs were in the R script so I've kept 
                    %them but they don't seem to be useful in trajectory plotting 
                    
                    %Calculate cumulative differences for each vector
                    zCumuSum = cumsum(abs(z));
                    xCumuSum = cumsum(abs(x));
                    tCumuSum = cumsum(abs(t));
                    
                    %Speed calculations (m/s)
                    subjSpeed = dists.*sampRate;
                    xSpeed = x.*sampRate;
                    zSpeed = z.*sampRate;

                    %Finding out which trial type it is
                    if strcmp(trialType, 'No Cues')
                        typeID = 1;
                        hasRepeatName(1) = 1;
                    elseif strcmp(trialType, 'Collocated')
                        typeID = 2;
                        hasRepeatName(2) = 1;
                    elseif strcmp(trialType, 'Combined')
                        typeID = 3;
                        hasRepeatName(3) = 1;
                    elseif strcmp(trialType, 'HUD')
                        typeID = 4;
                        hasRepeatName(4) = 1;
                    else
                        warning(strcat("Unknown Trial Type!!: ", trialType));
                        typeID = 5;
                    end

                    dataSetName = trialType;

                    %Plotting
                    %%NOTE - X IS FLIPPED BECAUSE THE SIDE-TO-SIDE
                    %%TRANSLATIONS ACROSS THE WIDTH OF THE HALLWAY IS
                    %%FLIPPED
                    plot(z, -x, 'LineWidth',1.25, 'Color', colours(typeID, :));
        
                    xlim([minX-0 maxX+0]);
                    ylim([-0.9 0.9]);
                    ax = gca;
                    ax.Clipping = 'off';
                    ax.FontName = 'Gill Sans MT';
                    ax.FontSize = 10;
                    
                    %X- and Y-Tick Marks
                    set(gca,'XTick', [0:1.5:15], 'YTick', [-0.9:0.3:0.9]);

                    %Flip Y-axis direction
                    %ax.YDir = 'reverse';

                    %Flips Y-Tick labels so that right = positive and left = negative
                    yt = get(gca, 'YTick');
                    set(gca, 'YTickLabel',flip(yt));
                    
                    %Tight border around the graph
                    set(gca,'LooseInset',get(gca,'TightInset'));

                    %Maps on HUD Cues on HUD condition trials
                    if(strcmp(trialType, 'HUD')||strcmp(trialType, 'Combined'))
                        fig = overlayHUDCues(fig, z, x, upHUD, downHUD, rightHUD, leftHUD, ax1);
                    end

                    %Maps on Obstacles
                    fig = overlayObstacles(fig, layoutNum);
                
                    %Surrounding Bars for Indicating where the Hallway is
                    borderX = 0:0.1:15; %ceil(maxX);
                    borderY1 = -0.9*ones(length(borderX), 1);
                    borderY2 = 0.9*ones(length(borderX), 1); 
                    plot(borderX, borderY1, 'k', 'Linewidth', 3, 'Color', '#737373');
                    plot(borderX, borderY2, 'k', 'Linewidth', 3, 'Color', '#737373');
                    
                    %Plotting the 1m scale marking
                    refX = 14:0.1:15;
                    refY = (borderY2(1)+0.15)*ones(length(refX), 1);
                    plot(refX, refY, 'k', 'Linewidth', 2);
                    
                    %Plotting the 1m scale text
                    txt = {'1m'};
                    t1 = text(14.7,refY(1)+0.15,txt);
                    t1.FontName = 'Gill Sans MT';
                    t1.FontSize = 14;
                    
                    %Plotting the START text
                    txt = {'START'};
                    t1 = text(-0.6, -0.3, txt);%text(0.25,borderY2(1)+0.25,txt);
                    t1.FontName = 'Gill Sans MT';
                    t1.FontSize = 12;
                    t1.Rotation = 90;
                
                    %Plotting name text
                    sbjFileName(5) = ' ';
                    nameText = strcat(sbjFileName, ' - Layout',' ', num2str(layoutNum));
                    t2 = text(0.25, -borderY2(1)-0.5, nameText);
                    t2.FontName = 'Gill Sans MT';
                    t2.FontSize = 12;
                
                    %Legend
                    lgd = legend(string(dataSetName), 'Location', [0.85 0.1 0.02 0.1]);
                    lgd.FontSize = 12;


                    %Defines folderpath to save the figures to
                    folderPath = '../PosFigures/singlePath_Condition/';
                    if ~exist(folderPath, 'dir')
                        mkdir(folderPath)
                    end
                
                    
                    %Saves the figure as a .png 
                    filePath = strcat('../PosFigures/singlePath_Condition/', sbjFileName, "_", trialType, "_Layout", num2str(layoutNum));
                    saveas(fig, strcat(filePath,'.fig'));
                    exportgraphics(fig,strcat(filePath,'.png'),'Resolution',900); %%For really high resolution pngs
                    %saveas(fig, strcat(filePath,'.png'));

                    counter = counter + 1;
                end
            end
        end
    end


end