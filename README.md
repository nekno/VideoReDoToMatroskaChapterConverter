VideoReDo To Matroska Chapter Converter
===================================

Converts VideoReDo chapter files to Ogg Media (.OGM) format for use in Matroska files made with mkvmerge/MKVToolNix.

The VideoReDo chapter format uses hours, minutes, seconds, and frames (`HH:MM:SS.ff`) to indicate each chapter mark.

Matroska video files (.MKV) created with mkvmerge take, as one option, chapters in OGM format, which uses hours, minutes, seconds, and milliseconds (`HH:MM:SS.mmm`).

VideoReDo To Matroska Chapter Converter converts one or more VideoReDo chapter files to OGM chapter files for use in MKV files. The output is an OGM chapter file with the same name as the VideoReDo chapter file, with the file extension changed to ".OGM.txt". The input VideoReDo chapter file is not changed.

The VideoReDo chapter file must have one of the following formats supported by VideoReDo:
* `HH:MM:SS,ff`
* `HH:MM:SS.ff`
* `HH:MM:SS;ff`
* `HH:MM:SS:ff`

For each batch, you must provide the framerate of the video file, which will be used to convert the number of frames to milliseconds. 

The framerate can be a number or a mathematical expression that evaluates to a more-precise framerate (e.g., `30000/1001` for 29.97fps, `24000/1001` for 23.98fps, or `60000/1001` for 59.94fps). The following operators are supported in the expression:
* + (addition)
* - (subtraction)
* * (multiplication)
* / (division)
* % (modulus) 
