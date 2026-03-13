import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi } from 'vitest';
import { Courses } from '../Courses';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockUseQuery = vi.fn();
vi.mock('@tanstack/react-query', () => ({
  useQuery: (...args: unknown[]) => mockUseQuery(...args),
}));

vi.mock('@/api/client', () => ({
  fetchCourses: vi.fn(),
}));

beforeEach(() => {
  mockUseQuery.mockReturnValue({ data: undefined, isLoading: false });
});

describe('Courses', () => {
  it('shows loading indicator while fetching', () => {
    mockUseQuery.mockReturnValue({ data: undefined, isLoading: true });
    render(<Courses />);
    expect(screen.getByText('common.loading')).toBeInTheDocument();
  });

  it('renders the page title', () => {
    mockUseQuery.mockReturnValue({ data: [], isLoading: false });
    render(<Courses />);
    expect(screen.getByText('courses.title')).toBeInTheDocument();
  });

  it('renders table headers', () => {
    mockUseQuery.mockReturnValue({ data: [], isLoading: false });
    render(<Courses />);
    expect(screen.getByText('courses.name')).toBeInTheDocument();
    expect(screen.getByText('courses.description')).toBeInTheDocument();
    expect(screen.getByText('courses.category')).toBeInTheDocument();
    expect(screen.getByText('courses.level')).toBeInTheDocument();
  });

  it('shows no-courses message when list is empty', () => {
    mockUseQuery.mockReturnValue({ data: [], isLoading: false });
    render(<Courses />);
    expect(screen.getByText('courses.noCourses')).toBeInTheDocument();
  });

  it('renders course data in table rows', () => {
    mockUseQuery.mockReturnValue({
      data: [{ id: 1, name: 'React Basics', description: 'Learn React', category: 'Frontend', level: 'Beginner' }],
      isLoading: false,
    });
    render(<Courses />);
    expect(screen.getByText('React Basics')).toBeInTheDocument();
    expect(screen.getByText('Learn React')).toBeInTheDocument();
    expect(screen.getByText('Frontend')).toBeInTheDocument();
    expect(screen.getByText('Beginner')).toBeInTheDocument();
  });

  it('renders "-" for null description, category and level', () => {
    mockUseQuery.mockReturnValue({
      data: [{ id: 2, name: 'Minimal Course', description: null, category: null, level: null }],
      isLoading: false,
    });
    render(<Courses />);
    expect(screen.getAllByText('-')).toHaveLength(3);
  });

  it('renders multiple courses', () => {
    mockUseQuery.mockReturnValue({
      data: [
        { id: 1, name: 'Course A', description: null, category: null, level: null },
        { id: 2, name: 'Course B', description: null, category: null, level: null },
      ],
      isLoading: false,
    });
    render(<Courses />);
    expect(screen.getByText('Course A')).toBeInTheDocument();
    expect(screen.getByText('Course B')).toBeInTheDocument();
  });
});
