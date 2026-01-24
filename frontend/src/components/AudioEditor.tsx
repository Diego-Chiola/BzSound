import React, { useState, useRef, useEffect } from "react";
import WaveSurfer from "wavesurfer.js";
import RegionsPlugin from "wavesurfer.js/dist/plugins/regions.js";
import axios from "axios";
import { AudioManipulator } from "../services/AudioManipulator";
import { Track } from "../types";

interface AudioEditorProps {
  token: string | null;
  userId: string | null;
}

const AudioEditor: React.FC<AudioEditorProps> = ({ token, userId }) => {
  const waveformRef = useRef<HTMLDivElement>(null);
  const wavesurferRef = useRef<WaveSurfer | null>(null);
  const regionsPluginRef = useRef<any>(null);
  const manipulatorRef = useRef<AudioManipulator>(new AudioManipulator());

  const [audioFile, setAudioFile] = useState<File | null>(null);
  const [editedBuffer, setEditedBuffer] = useState<AudioBuffer | null>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [loading, setLoading] = useState(false);
  const [region, setRegion] = useState<any>(null);
  const [tracks, setTracks] = useState<Track[]>([]);

  useEffect(() => {
    if (waveformRef.current && !wavesurferRef.current) {
      regionsPluginRef.current = RegionsPlugin.create();

      wavesurferRef.current = WaveSurfer.create({
        container: waveformRef.current,
        waveColor: "#ddd",
        progressColor: "#667eea",
        cursorColor: "#764ba2",
        barWidth: 2,
        barRadius: 3,
        height: 128,
        plugins: [regionsPluginRef.current],
      });

      wavesurferRef.current.on("play", () => setIsPlaying(true));
      wavesurferRef.current.on("pause", () => setIsPlaying(false));
      wavesurferRef.current.on("timeupdate", (time: number) =>
        setCurrentTime(time),
      );
      wavesurferRef.current.on("ready", () => {
        if (wavesurferRef.current) {
          setDuration(wavesurferRef.current.getDuration());
        }
      });

      regionsPluginRef.current.on("region-created", (region: any) => {
        setRegion(region);
      });
    }

    return () => {
      if (wavesurferRef.current) {
        wavesurferRef.current.destroy();
        wavesurferRef.current = null;
      }
    };
  }, []);

  useEffect(() => {
    if (token && userId) {
      loadUserTracks();
    }
  }, [token, userId]);

  const loadUserTracks = async () => {
    if (!token || !userId) return;

    try {
      const response = await axios.get<Track[]>(`/api/users/${userId}/tracks`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setTracks(response.data);
    } catch (error) {
      console.error("Error loading tracks:", error);
    }
  };

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setLoading(true);
    setAudioFile(file);

    try {
      // Load into manipulator
      await manipulatorRef.current.loadAudioFile(file);

      // Load into waveform
      const url = URL.createObjectURL(file);
      wavesurferRef.current?.load(url);
    } catch (error) {
      alert("Error loading audio file");
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handlePlayPause = () => {
    wavesurferRef.current?.playPause();
  };

  const handleStop = () => {
    wavesurferRef.current?.stop();
    setIsPlaying(false);
  };

  const handleMarkRegion = () => {
    if (!wavesurferRef.current || !regionsPluginRef.current) return;

    // Clear existing regions
    regionsPluginRef.current.clearRegions();

    const newRegion = regionsPluginRef.current.addRegion({
      start: currentTime,
      end: Math.min(currentTime + 5, duration),
      color: "rgba(102, 126, 234, 0.3)",
      drag: true,
      resize: true,
    });

    setRegion(newRegion);
  };

  const handleCut = async () => {
    if (!region) {
      alert("Please select a region first");
      return;
    }

    setLoading(true);
    try {
      const cutBuffer = manipulatorRef.current.cutSegment(
        region.start,
        region.end,
      );
      setEditedBuffer(cutBuffer);

      // Load cut segment into waveform
      const blob = manipulatorRef.current.audioBufferToWav(cutBuffer);
      const url = URL.createObjectURL(blob);
      wavesurferRef.current?.load(url);

      alert("Segment cut successfully!");
    } catch (error) {
      alert("Error cutting segment");
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleRemove = async () => {
    if (!region) {
      alert("Please select a region first");
      return;
    }

    setLoading(true);
    try {
      const newBuffer = manipulatorRef.current.removeSegment(
        region.start,
        region.end,
      );
      setEditedBuffer(newBuffer);

      // Load new buffer into waveform
      const blob = manipulatorRef.current.audioBufferToWav(newBuffer);
      const url = URL.createObjectURL(blob);
      wavesurferRef.current?.load(url);

      regionsPluginRef.current?.clearRegions();
      setRegion(null);

      alert("Segment removed successfully!");
    } catch (error) {
      alert("Error removing segment");
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleLoop = async () => {
    if (!region) {
      alert("Please select a region first");
      return;
    }

    const repetitions = prompt("How many times to loop?", "3");
    if (!repetitions) return;

    setLoading(true);
    try {
      const loopedBuffer = manipulatorRef.current.loopSegment(
        region.start,
        region.end,
        parseInt(repetitions),
      );
      setEditedBuffer(loopedBuffer);

      // Load looped buffer into waveform
      const blob = manipulatorRef.current.audioBufferToWav(loopedBuffer);
      const url = URL.createObjectURL(blob);
      wavesurferRef.current?.load(url);

      alert("Segment looped successfully!");
    } catch (error) {
      alert("Error looping segment");
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = () => {
    if (!editedBuffer && !audioFile) {
      alert("No audio to download");
      return;
    }

    const buffer =
      editedBuffer || manipulatorRef.current.cutSegment(0, duration);
    const filename =
      audioFile?.name.replace(/\.[^/.]+$/, "") + "-edited.wav" ||
      "edited-audio.wav";
    manipulatorRef.current.downloadAudio(buffer, filename);
  };

  const handleSaveToCloud = async () => {
    if (!token || !userId) {
      alert("Please login to save files");
      return;
    }

    if (!editedBuffer && !audioFile) {
      alert("No audio to save");
      return;
    }

    const title = prompt(
      "Enter track title:",
      audioFile?.name.replace(/\.[^/.]+$/, "") || "My Track",
    );
    if (!title) return;

    setLoading(true);
    try {
      const buffer =
        editedBuffer || manipulatorRef.current.cutSegment(0, duration);
      const file = manipulatorRef.current.audioBufferToFile(
        buffer,
        title + ".wav",
      );

      const formData = new FormData();
      formData.append("file", file);
      formData.append("title", title);

      await axios.post(`/api/users/${userId}/tracks/upload`, formData, {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "multipart/form-data",
        },
      });

      alert("Track saved successfully!");
      loadUserTracks();
    } catch (error: any) {
      alert(
        "Error saving track: " +
          (error.response?.data?.message || error.message),
      );
    } finally {
      setLoading(false);
    }
  };

  const handleLoadTrack = async (track: Track) => {
    setLoading(true);
    try {
      const response = await axios.get(track.filePath, {
        responseType: "blob",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });

      const file = new File([response.data], track.title, {
        type: track.contentType,
      });
      setAudioFile(file);

      // Load into manipulator
      await manipulatorRef.current.loadAudioFile(file);

      // Load into waveform
      const url = URL.createObjectURL(file);
      wavesurferRef.current?.load(url);
    } catch (error) {
      alert("Error loading track");
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

  return (
    <div className="w-full">
      <div className="bg-white rounded-2xl p-8 shadow-xl">
        <h2 className="m-0 mb-5 text-primary text-3xl font-bold">
          Audio Editor
        </h2>

        <div className="mb-8 flex items-center gap-4">
          <input
            type="file"
            accept="audio/*"
            onChange={handleFileSelect}
            id="audio-file"
            className="hidden"
          />
          <label
            htmlFor="audio-file"
            className="inline-block px-6 py-3 bg-primary text-white rounded-lg cursor-pointer font-semibold transition-all hover:bg-primary-dark hover:-translate-y-0.5 hover:shadow-lg"
          >
            📁 Choose Audio File
          </label>
          {audioFile && (
            <span className="text-gray-600 italic">{audioFile.name}</span>
          )}
        </div>

        {audioFile && (
          <>
            <div className="relative bg-gray-100 rounded-lg p-5 mb-5">
              <div ref={waveformRef} className="w-full" />
              {loading && (
                <div className="absolute inset-0 bg-white bg-opacity-90 flex items-center justify-center text-xl text-primary font-semibold rounded-lg">
                  Loading...
                </div>
              )}
            </div>

            <div className="text-center text-lg text-gray-600 mb-5 font-mono">
              <span className="mx-1">{formatTime(currentTime)}</span>
              <span className="mx-1">/</span>
              <span className="mx-1">{formatTime(duration)}</span>
            </div>

            <div className="flex flex-wrap gap-2 justify-center mb-5">
              <button
                onClick={handlePlayPause}
                className="px-5 py-3 border-2 border-primary bg-primary text-white rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary-dark hover:-translate-y-0.5 hover:shadow-lg"
              >
                {isPlaying ? "⏸ Pause" : "▶ Play"}
              </button>
              <button
                onClick={handleStop}
                className="px-5 py-3 border-2 border-primary bg-white text-primary rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-lg"
              >
                ⏹ Stop
              </button>
              <button
                onClick={handleMarkRegion}
                className="px-5 py-3 border-2 border-primary bg-white text-primary rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-lg"
              >
                ✂️ Mark Region
              </button>
              <button
                onClick={handleCut}
                disabled={!region}
                className="px-5 py-3 border-2 border-primary bg-white text-primary rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
              >
                ✂️ Cut
              </button>
              <button
                onClick={handleRemove}
                disabled={!region}
                className="px-5 py-3 border-2 border-primary bg-white text-primary rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
              >
                🗑️ Remove
              </button>
              <button
                onClick={handleLoop}
                disabled={!region}
                className="px-5 py-3 border-2 border-primary bg-white text-primary rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
              >
                🔁 Loop
              </button>
              <button
                onClick={handleDownload}
                className="px-5 py-3 border-2 border-primary bg-white text-primary rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-lg"
              >
                💾 Download
              </button>
              <button
                onClick={handleSaveToCloud}
                disabled={!token || loading}
                className="px-5 py-3 border-2 border-green-600 bg-green-600 text-white rounded-lg text-base font-semibold cursor-pointer transition-all hover:bg-green-700 hover:border-green-700 hover:-translate-y-0.5 hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
              >
                ☁️ Save to Cloud
              </button>
            </div>

            {!token && (
              <p className="text-center p-4 bg-amber-100 rounded-lg text-amber-900 font-medium mt-5">
                💡 Login to save your edited files to the cloud!
              </p>
            )}
          </>
        )}

        {token && tracks.length > 0 && (
          <div className="mt-8 pt-8 border-t-2 border-gray-300">
            <h3 className="my-0 mb-4 text-primary text-2xl font-bold">
              My Tracks
            </h3>
            <ul className="list-none p-0 m-0">
              {tracks.map((track) => (
                <li
                  key={track.id}
                  onClick={() => handleLoadTrack(track)}
                  className="p-4 bg-gray-100 rounded-lg mb-2 cursor-pointer flex justify-between items-center transition-all hover:bg-gray-200 hover:translate-x-1"
                >
                  <span className="font-semibold text-gray-800">
                    {track.title}
                  </span>
                  <span className="text-gray-600 text-sm">
                    {new Date(track.lastModified).toLocaleDateString()}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
};

export default AudioEditor;
