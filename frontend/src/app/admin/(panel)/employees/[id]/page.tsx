import EmployeeInfo from '@/components/admin/employees/details/EmployeeInfo';

export default async function AdminEmployeeDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <>
      <EmployeeInfo employeeId={id} />
    </>
  );
}
