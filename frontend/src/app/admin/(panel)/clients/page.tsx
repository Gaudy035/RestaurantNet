import AdminClientsMain from '@/components/clients/ClientsMain';
import AdminClientsSearchBar from '@/components/clients/ClientsSearchBar';
import { buttonVariants } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import Link from 'next/link';

export default async function AdminClientsPage({
  searchParams,
}: {
  searchParams: Promise<{ param?: string }>;
}) {
  const { param } = await searchParams;
  return (
    <div className='flex flex-1 flex-col'>
      <div className='flex justify-end items-center gap-2 m-4'>
        <AdminClientsSearchBar val={param} />
        <Link
          href='/admin/clients/add'
          className={cn(
            buttonVariants({ variant: 'default', size: 'default' }),
          )}
        >
          Add new user
        </Link>
      </div>
      That's the client page
      <AdminClientsMain clientData={param} />
    </div>
  );
}
