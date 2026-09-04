import { buttonVariants } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import AdminEmployeesSearchBar from '@/components/employees/EmployeesSearchBar';
import AdminEmployeesMain from '@/components/employees/EmployeesMain';

export default async function AdminEmployeesPage({
  searchParams,
}: {
  searchParams: Promise<{ param?: string }>;
}) {
  const { param } = await searchParams;
  return (
    <div className='flex flex-1 flex-col m-4'>
      <div className='flex justify-between items-center'>
        <h1 className='text-2xl'>Employees</h1>
        <div className='flex justify-end items-center gap-2'>
          <AdminEmployeesSearchBar val={param} />
          <Link
            href='/admin/employees/add'
            className={cn(
              buttonVariants({ variant: 'default', size: 'default' }),
            )}
          >
            Add new employee
          </Link>
        </div>
      </div>
      <AdminEmployeesMain employeeData={param} />
    </div>
  );
}
