import AdminClientsMain from '@/components/clients/ClientsMain';
import AdminClientsSearchBar from '@/components/clients/ClientsSearchBar';

export default async function AdminClientsPage({
  searchParams,
}: {
  searchParams: Promise<{ param?: string }>;
}) {
  const { param } = await searchParams;
  return (
    <div className='flex flex-1 flex-col'>
      <div className='flex justify-end items-center'>
        <AdminClientsSearchBar val={param} />
      </div>
      That's the client page
      <AdminClientsMain clientData={param} />
    </div>
  );
}
