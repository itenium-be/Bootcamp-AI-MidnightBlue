import axios from 'axios';
import { useAuthStore } from '../stores';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  },
);

interface LoginResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
}

export async function loginApi(username: string, password: string): Promise<LoginResponse> {
  const params = new URLSearchParams();
  params.append('grant_type', 'password');
  params.append('username', username);
  params.append('password', password);
  params.append('client_id', 'skillforge-spa');
  params.append('scope', 'openid profile email');

  const response = await axios.post<LoginResponse>(`${API_BASE_URL}/connect/token`, params, {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  });

  return response.data;
}

interface Team {
  id: number;
  name: string;
}

export async function fetchUserTeams(): Promise<Team[]> {
  const response = await api.get<Team[]>('/api/team');
  return response.data;
}

interface Course {
  id: number;
  name: string;
  description: string | null;
  category: string | null;
  level: string | null;
}

export async function fetchCourses(): Promise<Course[]> {
  const response = await api.get<Course[]>('/api/course');
  return response.data;
}

export interface SkillSummary {
  id: number;
  name: string;
  category: string;
  description: string | null;
  levelCount: number;
}

export interface SkillLevelDescriptor {
  level: number;
  description: string;
}

export interface SkillPrerequisite {
  requiredSkillId: number;
  requiredSkillName: string;
  requiredLevel: number;
}

export interface SkillDetail extends SkillSummary {
  levelDescriptors: SkillLevelDescriptor[];
  prerequisites: SkillPrerequisite[];
}

export interface SkillCategory {
  category: string;
  skills: SkillSummary[];
}

export interface RoadmapSkill extends SkillSummary {
  unmetPrerequisites: SkillPrerequisite[];
}

export interface RoadmapCategory {
  category: string;
  skills: RoadmapSkill[];
}

export interface CompetenceCentreProfile {
  id: number;
  name: string;
  description: string | null;
  skillCount: number;
}

export async function fetchSkillCatalogue(): Promise<SkillCategory[]> {
  const response = await api.get<SkillCategory[]>('/api/skill/catalogue');
  return response.data;
}

export async function fetchSkill(id: number): Promise<SkillDetail> {
  const response = await api.get<SkillDetail>(`/api/skill/${id}`);
  return response.data;
}

export async function fetchProfiles(): Promise<CompetenceCentreProfile[]> {
  const response = await api.get<CompetenceCentreProfile[]>('/api/profile');
  return response.data;
}

export async function fetchProfileSkills(profileId: number): Promise<SkillSummary[]> {
  const response = await api.get<SkillSummary[]>(`/api/profile/${profileId}/skills`);
  return response.data;
}

export interface ConsultantSummary {
  userId: string;
  displayName: string;
  email: string;
  teamId: number;
  teamName: string;
  lastActivityAt: string | null;
  isInactive: boolean;
  daysSinceActivity: number | null;
}

export interface ConsultantDetail extends ConsultantSummary {
  createdAt: string;
  profileId: number | null;
  profileName: string | null;
}

export async function fetchConsultants(): Promise<ConsultantSummary[]> {
  const response = await api.get<ConsultantSummary[]>('/api/consultant');
  return response.data;
}

export async function fetchConsultant(userId: string): Promise<ConsultantDetail> {
  const response = await api.get<ConsultantDetail>(`/api/consultant/${userId}`);
  return response.data;
}

export async function assignConsultantProfile(userId: string, profileId: number | null): Promise<void> {
  await api.put(`/api/consultant/${userId}/profile`, { profileId });
}

export async function fetchConsultantSkills(userId: string): Promise<RoadmapCategory[]> {
  const response = await api.get<RoadmapCategory[]>(`/api/consultant/${userId}/skills`);
  return response.data;
}
