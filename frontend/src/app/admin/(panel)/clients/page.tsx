import AdminClientsMain from '@/components/clients/ClientsMain';
import AdminClientsSearchBar from '@/components/clients/ClientsSearchBar';

export default async function AdminClientsPage({
  searchParams,
}: {
  searchParams: Promise<{ param?: string }>;
}) {
  const { param } = await searchParams;
  return (
    <div>
      <AdminClientsSearchBar val={param} />
      That's the client page
      <AdminClientsMain clientData={param} />
    </div>
  );
}
