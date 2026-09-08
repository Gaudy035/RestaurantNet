import AdminClientsMain from '@/components/admin/clients/ClientsMain';
import AdminClientsSearchBar from '@/components/admin/clients/ClientsSearchBar';
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
    <div className='flex flex-1 flex-col m-4'>
      <div className='flex justify-between items-center'>
        <h1 className='text-2xl'>Clients</h1>
        <div className='flex justify-end items-center gap-2'>
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
      </div>
      <AdminClientsMain clientData={param} />
    </div>
  );
}
