import { createFileRoute } from '@tanstack/react-router';
import { ConsultantProfile } from '@/pages/ConsultantProfile';

export const Route = createFileRoute('/_authenticated/consultant/$userId')({
  component: function ConsultantProfileRoute() {
    const { userId } = Route.useParams();
    return <ConsultantProfile userId={userId} />;
  },
});
