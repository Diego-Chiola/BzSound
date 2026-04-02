export interface Track {
  id: number;
  title: string;
  lastModified: string;
  filePath: string;
  fileSize: number;
  contentType: string;
  duration?: number;
  userId?: string;
}

export interface CreateTrackRequest {
  title: string;
  filePath: string;
}

export interface UpdateTrackRequest {
  title?: string;
  filePath?: string;
}

export interface User {
  id: string;
  email: string;
  username: string;
}

export interface AuthRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  email: string;
  token: string;
}
