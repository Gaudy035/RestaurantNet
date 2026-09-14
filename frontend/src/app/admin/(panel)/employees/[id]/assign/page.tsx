import { AdminEmployeeAssignForm } from '@/components/admin/employees/details/EmployeeAssignForm';

export default async function AdminClientAddPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return (
    <div className='flex flex-1 justify-center items-center'>
      <div className='w-full max-w-sm'>
        <AdminEmployeeAssignForm employeeId={id} />
      </div>
    </div>
  );
}
