import OrderInfo from '@/components/admin/orders/details/OrderInfo';

export default async function AdminOrderDetailsPage({
    params,
}: {
    params: Promise<{ id: string }>;
}) {
    const { id } = await params;

    return (
        <div className='flex flex-1 flex-col m-4 gap-4'>
            <OrderInfo orderId={id} />
        </div>
    );
}
