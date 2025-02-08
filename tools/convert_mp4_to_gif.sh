#!/bin/bash
while IFS=$'\t' read -r status video; do
gif_file="${video/videos/images}"
gif_file="${gif_file%.mp4}.gif"
if [[ "$status" == "A" || "$status" == "M" ]]; then
    echo "Converting $video to $gif_file"
    echo "ffmpeg -y -i \"$video\" -vf \"fps=10,scale=iw:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\" -c:v gif \"$gif_file\""
    ffmpeg -y -loglevel repeat+level+verbose -i "$video" -vf "fps=10,scale=iw:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" -c:v gif "$gif_file"
    if [[ $? -ne 0 ]]; then
      echo "Error converting $video to $gif_file" >&2
    fi
elif [[ "$status" == "D" ]]; then
    if [[ -f "$gif_file" ]]; then
        echo "Deleting $gif_file"
        rm "$gif_file"
    else
        echo "$gif_file does not exist, no action taken."
    fi
fi
done < changed_videos.txt