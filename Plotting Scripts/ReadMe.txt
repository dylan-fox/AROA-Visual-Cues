Set-Up
- Put all raw data files into a folder named "PosRawData" adjacent to the PositionTrackingScripts folder where the scripts are
ex. if this folder is located at C:/Documents/Plotting/PositionTrackingScripts, then put all the raw data files into 
C:/Documents/RawData

- Keep the .txt files sorted in the folders sorted by participant (the way they come from the Google Drive)
ex. if you were to access a file the path would be 
C:/Documents/RawData/AR01_11-17-21/2021-11-17_03-48-14-PM_No Cues_Layout 6.txt

- The Matlab plotting scripts will create a new folders called "PosPCAData" and "PositionPlots" inside whatever folder 
you put the PositionTrackingScripts folder in



To run position tracking:
1. Put all folders of raw data into the PositionTracking/Data/raw data folder. 
Keep the data in their folders, ie. PositionTracking/Data/raw data/ARXX_2021_xx_xx

2. Run export_PCA_alt.m to convert data to a graphable form

3. Run posTrackPlot.m to graphs position tracking plots. These will plot them according to participant, 
but can be modifed to sort data according to condition, etc.