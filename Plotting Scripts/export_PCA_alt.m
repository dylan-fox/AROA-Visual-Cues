clear all;
close all;

%Sets directory path to the raw data folder
datapath = '../PosRawData/';%'./Data/raw data/';

% subject directory list
listing = dir(datapath);
listing = listing(3:end); 

%Counter for debugging purposes
counter = 1;
for s = 1:length(listing); %goes through all folders
    
    if listing(s).isdir
        
        dirname = listing(s).name;
        
        % file list
        files = dir([datapath dirname]);
        
        if isempty(strfind(dirname,'exclude'))
            %d = 'etnered'
            for f = 1:length(files)
               % d = 'etnered2'
                if strfind(files(f).name,'txt')
                   d = files(f).name;
                   
                    task = 'posTracking';

                    %assigns subjectID value as the first 3 digits of the filename
                    %sbj = files(f).name(1:4); 
    
                    
                    %TODO - change sbj naming
                    sbjFileName = strcat(dirname, files(f).name(23:end-4));


                    % read in data from txt
                    fileID = fopen([ datapath dirname '/' files(f).name]);

                    C = textscan(fileID,['%s' '%s' repmat('%f',[1 7]) '%s' '%s' repmat('%f',[1 6]) repmat('%s',[1 4])],'Delimiter',';','HeaderLines',2);
              
                    fclose(fileID);

                    % store position and time information
                    x = C{4};
                    y = C{5};
                    z = C{6};
                    t = C{3};
                    
                    %remove the last value from each array because it's a
                    %NaN from the "Logging ended at xyz" line at the end
                    x = x(1:end-1);
                    y = y(1:end-1);
                    z = z(1:end-1);
                    t = t(1:end-1);
                    
                    % find axis of maximal variance
                    [coeff,~,~,~,explained,~] = pca([x , z]);
                    coeff = coeff(:,1);

                    theta = atan2( coeff(1), coeff(2) );
                    A = [ cos( theta ) sin( theta );
                        -sin( theta ) cos( theta );];
                    newax = [x , z] * A;
                    x = newax(:,1);
                    z = newax(:,2);

                    % make initial t, x and z zero, with increasing values
                    %z = abs(z - z(1)); % down hall always positive
                    z = z - z(1);
                    x = x - x(1);
                    t = t - t(1);
                    
                    % if z is pointing in the wrong direction rotate 180 deg
                    if sign(z(end)) == -1
                        z = -z;
                        x = -x;
                    end

                    %Also store the HUD bools while converting "False" to 0
                    %and "True" to 1 and also converting it all into doubles
                    
                    upHUD = str2double(strrep(strrep(C{18}, 'False', '0'), 'True', '1'));
                    rightHUD = str2double(strrep(strrep(C{19}, 'False', '0'), 'True', '1'));
                    downHUD = str2double(strrep(strrep(C{20}, 'False', '0'), 'True', '1'));
                    leftHUD = str2double(strrep(strrep(C{21}, 'False', '0'), 'True', '1'));
                    
                    %This is what the lines above are doing
%                     upHUD = C{18};
%                     upHUD = strrep(upHUD, 'False', '0');
%                     upHUD = strrep(upHUD, 'True', '1');


                    
                    %position-tracking datafiles are made
                    
                    folderPath = strcat('../PosPCAData/',dirname, '/');

                    if ~exist(folderPath, 'dir')
                        mkdir(folderPath)
                    end
                    csvname = strcat(folderPath, sbjFileName, '_',num2str(task),'_', '.csv');
                    %1:end-1 because that's what's done to z, x, and t to
                    %remove the "NaN' at the end
                    csvwrite(csvname, [z,x,t, upHUD(1:end-1), rightHUD(1:end-1), downHUD(1:end-1), leftHUD(1:end-1)]);

                   counter = counter +1;

                end
            end
            
        end
        
        
    end
    
end