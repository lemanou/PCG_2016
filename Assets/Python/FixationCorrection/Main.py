import DataClustering as dc
import BlinksPlotting as bp

path = "2016.05.21/Malene/"
scene_swap = 1


if scene_swap == 1:
    bli_input_files = {"Blinks For scene2.csv", "Blinks For LoadSavedLevel1.csv"}
    fix_input_files = {"Gazes For scene2.csv", "Gazes For LoadSavedLevel1.csv"}
else:
    bli_input_files = {"Blinks For scene1.csv", "Blinks For LoadSavedLevel2.csv"}
    fix_input_files = {"Gazes For scene1.csv", "Gazes For LoadSavedLevel2.csv"}
plot_EVs = False  # change to True to calculate and plot the explained variance instead of the blinks

for tmp_file in bli_input_files:
    bp.all_blinks(tmp_file, plot_EVs, path)
    if not plot_EVs:
        bp.normal_against_long(tmp_file, path)

if not plot_EVs:
    for tmp_file in fix_input_files:
        dc.start(tmp_file, path)
