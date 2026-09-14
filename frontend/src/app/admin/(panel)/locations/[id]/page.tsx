import LocationInfo from '@/components/admin/locations/details/LocationInfo';
import LocationEmployees from '@/components/admin/locations/details/LocationEmployees';

export default async function AdminLocationDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <div className='flex flex-1 flex-col m-4 gap-4'>
      <LocationInfo locationId={id} />
      <LocationEmployees locationId={id} />
    </div>
  );
}
