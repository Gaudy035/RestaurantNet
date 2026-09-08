import { buttonVariants } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import Link from 'next/link';

export default async function AdminEmployeesPage({
  searchParams,
}: {
  searchParams: Promise<{ param?: string }>;
}) {
  const { param } = await searchParams;
  return (
    <div className='flex flex-1 flex-col m-4'>
      <div className='flex justify-between items-center'>
        <h1 className='text-2xl'>Locations</h1>
        <div className='flex justify-end items-center gap-2'>
          {/* Search bar here */}
          <Link
            href='/admin/locations/add'
            className={cn(
              buttonVariants({ variant: 'default', size: 'default' }),
            )}
          >
            Add new location
          </Link>
        </div>
      </div>
      {/* Main here */}
    </div>
  );
}
