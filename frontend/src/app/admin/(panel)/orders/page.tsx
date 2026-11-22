import AdminOrdersFilterBar from '@/components/admin/orders/OrdersFilterBar';
import AdminOrdersMain from '@/components/admin/orders/OrdersMain';

export default async function AdminOrdersPage({
    searchParams,
}: {
    searchParams: Promise<{ date?: string; locationId?: string }>;
}) {
    const { date, locationId } = await searchParams;

    return (
        <div className='flex flex-1 flex-col m-4'>
            <div className='flex justify-between items-center'>
                <h1 className='text-2xl'>Orders</h1>
                <div className='flex justify-end items-center gap-2'>
                    <AdminOrdersFilterBar
                        key={`${date ?? ''}-${locationId ?? ''}`}
                        date={date}
                        locationId={locationId}
                    />
                </div>
            </div>
            <AdminOrdersMain
                key={`${date ?? ''}-${locationId ?? ''}`}
                date={date}
                locationId={locationId}
            />
        </div>
    );
}
