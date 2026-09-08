import LocationInfo from '@/components/admin/locations/details/LocationInfo';

export default async function AdminLocationDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <div className='flex flex-1 flex-col m-4'>
      <LocationInfo locationId={id} />
    </div>
  );
}
