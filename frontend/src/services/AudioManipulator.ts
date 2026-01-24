/**
 * Audio Manipulation Service
 * Provides methods for cutting, looping, and exporting audio using Web Audio API
 */

export class AudioManipulator {
  private audioContext: AudioContext;
  private audioBuffer: AudioBuffer | null = null;

  constructor() {
    this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
  }

  /**
   * Load audio file into AudioBuffer
   */
  async loadAudioFile(file: File): Promise<AudioBuffer> {
    const arrayBuffer = await file.arrayBuffer();
    this.audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
    return this.audioBuffer;
  }

  /**
   * Cut a segment from the audio
   */
  cutSegment(startTime: number, endTime: number): AudioBuffer {
    if (!this.audioBuffer) {
      throw new Error('No audio loaded');
    }

    const sampleRate = this.audioBuffer.sampleRate;
    const numChannels = this.audioBuffer.numberOfChannels;
    
    const startSample = Math.floor(startTime * sampleRate);
    const endSample = Math.floor(endTime * sampleRate);
    const segmentLength = endSample - startSample;

    const newBuffer = this.audioContext.createBuffer(
      numChannels,
      segmentLength,
      sampleRate
    );

    for (let channel = 0; channel < numChannels; channel++) {
      const sourceData = this.audioBuffer.getChannelData(channel);
      const newData = newBuffer.getChannelData(channel);
      
      for (let i = 0; i < segmentLength; i++) {
        newData[i] = sourceData[startSample + i];
      }
    }

    return newBuffer;
  }

  /**
   * Remove a segment from the audio (cut out)
   */
  removeSegment(startTime: number, endTime: number): AudioBuffer {
    if (!this.audioBuffer) {
      throw new Error('No audio loaded');
    }

    const sampleRate = this.audioBuffer.sampleRate;
    const numChannels = this.audioBuffer.numberOfChannels;
    
    const startSample = Math.floor(startTime * sampleRate);
    const endSample = Math.floor(endTime * sampleRate);
    const totalSamples = this.audioBuffer.length;
    const newLength = totalSamples - (endSample - startSample);

    const newBuffer = this.audioContext.createBuffer(
      numChannels,
      newLength,
      sampleRate
    );

    for (let channel = 0; channel < numChannels; channel++) {
      const sourceData = this.audioBuffer.getChannelData(channel);
      const newData = newBuffer.getChannelData(channel);
      
      // Copy before cut
      for (let i = 0; i < startSample; i++) {
        newData[i] = sourceData[i];
      }
      
      // Copy after cut
      for (let i = endSample; i < totalSamples; i++) {
        newData[i - (endSample - startSample)] = sourceData[i];
      }
    }

    return newBuffer;
  }

  /**
   * Loop a segment multiple times
   */
  loopSegment(startTime: number, endTime: number, repetitions: number): AudioBuffer {
    if (!this.audioBuffer) {
      throw new Error('No audio loaded');
    }

    const sampleRate = this.audioBuffer.sampleRate;
    const numChannels = this.audioBuffer.numberOfChannels;
    
    const startSample = Math.floor(startTime * sampleRate);
    const endSample = Math.floor(endTime * sampleRate);
    const segmentLength = endSample - startSample;
    const newLength = segmentLength * repetitions;

    const newBuffer = this.audioContext.createBuffer(
      numChannels,
      newLength,
      sampleRate
    );

    for (let channel = 0; channel < numChannels; channel++) {
      const sourceData = this.audioBuffer.getChannelData(channel);
      const newData = newBuffer.getChannelData(channel);
      
      for (let rep = 0; rep < repetitions; rep++) {
        const offset = rep * segmentLength;
        for (let i = 0; i < segmentLength; i++) {
          newData[offset + i] = sourceData[startSample + i];
        }
      }
    }

    return newBuffer;
  }

  /**
   * Fade in effect
   */
  fadeIn(buffer: AudioBuffer, duration: number = 0.5): AudioBuffer {
    const sampleRate = buffer.sampleRate;
    const fadeSamples = Math.floor(duration * sampleRate);
    const numChannels = buffer.numberOfChannels;

    for (let channel = 0; channel < numChannels; channel++) {
      const data = buffer.getChannelData(channel);
      
      for (let i = 0; i < fadeSamples && i < data.length; i++) {
        const gain = i / fadeSamples;
        data[i] *= gain;
      }
    }

    return buffer;
  }

  /**
   * Fade out effect
   */
  fadeOut(buffer: AudioBuffer, duration: number = 0.5): AudioBuffer {
    const sampleRate = buffer.sampleRate;
    const fadeSamples = Math.floor(duration * sampleRate);
    const numChannels = buffer.numberOfChannels;

    for (let channel = 0; channel < numChannels; channel++) {
      const data = buffer.getChannelData(channel);
      const startSample = data.length - fadeSamples;
      
      for (let i = 0; i < fadeSamples && startSample + i < data.length; i++) {
        const gain = 1 - (i / fadeSamples);
        data[startSample + i] *= gain;
      }
    }

    return buffer;
  }

  /**
   * Convert AudioBuffer to WAV Blob
   */
  audioBufferToWav(buffer: AudioBuffer): Blob {
    const numChannels = buffer.numberOfChannels;
    const sampleRate = buffer.sampleRate;
    const format = 1; // PCM
    const bitDepth = 16;

    const bytesPerSample = bitDepth / 8;
    const blockAlign = numChannels * bytesPerSample;

    const data: number[] = [];
    for (let i = 0; i < buffer.length; i++) {
      for (let channel = 0; channel < numChannels; channel++) {
        const sample = buffer.getChannelData(channel)[i];
        const int16 = Math.max(-1, Math.min(1, sample));
        data.push(int16 < 0 ? int16 * 0x8000 : int16 * 0x7FFF);
      }
    }

    const dataLength = data.length * bytesPerSample;
    const headerLength = 44;
    const totalLength = headerLength + dataLength;

    const arrayBuffer = new ArrayBuffer(totalLength);
    const view = new DataView(arrayBuffer);

    // RIFF chunk descriptor
    this.writeString(view, 0, 'RIFF');
    view.setUint32(4, totalLength - 8, true);
    this.writeString(view, 8, 'WAVE');

    // FMT sub-chunk
    this.writeString(view, 12, 'fmt ');
    view.setUint32(16, 16, true);
    view.setUint16(20, format, true);
    view.setUint16(22, numChannels, true);
    view.setUint32(24, sampleRate, true);
    view.setUint32(28, sampleRate * blockAlign, true);
    view.setUint16(32, blockAlign, true);
    view.setUint16(34, bitDepth, true);

    // Data sub-chunk
    this.writeString(view, 36, 'data');
    view.setUint32(40, dataLength, true);

    // Write audio data
    let offset = 44;
    for (let i = 0; i < data.length; i++) {
      view.setInt16(offset, data[i], true);
      offset += 2;
    }

    return new Blob([arrayBuffer], { type: 'audio/wav' });
  }

  private writeString(view: DataView, offset: number, string: string): void {
    for (let i = 0; i < string.length; i++) {
      view.setUint8(offset + i, string.charCodeAt(i));
    }
  }

  /**
   * Download audio buffer as WAV file
   */
  downloadAudio(buffer: AudioBuffer, filename: string = 'edited-audio.wav'): void {
    const blob = this.audioBufferToWav(buffer);
    const url = URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    
    URL.revokeObjectURL(url);
  }

  /**
   * Convert AudioBuffer to File for upload
   */
  audioBufferToFile(buffer: AudioBuffer, filename: string = 'edited-audio.wav'): File {
    const blob = this.audioBufferToWav(buffer);
    return new File([blob], filename, { type: 'audio/wav' });
  }
}
