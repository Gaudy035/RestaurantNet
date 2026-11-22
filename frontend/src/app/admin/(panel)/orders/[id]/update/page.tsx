import AdminOrderUpdateForm from '@/components/admin/orders/details/OrderUpdateForm';

export default async function AdminOrderUpdatePage({
    params,
}: {
    params: Promise<{ id: string }>;
}) {
    const { id } = await params;

    return (
        <div className='flex flex-1 justify-center items-center'>
            <div className='w-full max-w-sm'>
                <AdminOrderUpdateForm orderId={id} />
            </div>
        </div>
    );
}
