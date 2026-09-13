import EmployeeInfo from '@/components/admin/employees/details/EmployeeInfo';

export default async function AdminEmployeeDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <div className='flex flex-1 flex-col m-4'>
      <EmployeeInfo employeeId={id} />
    </div>
  );
}
