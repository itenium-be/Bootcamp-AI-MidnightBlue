import { createFileRoute } from '@tanstack/react-router';
import { MyGoals } from '@/pages/MyGoals';

export const Route = createFileRoute('/_authenticated/goals')({
  component: MyGoals,
});
