import AdminCartMain from '@/components/admin/cart/CartMain';

export default function AdminCartPage() {
    return (
        <div className='flex flex-1 flex-col m-4'>
            <div className='flex justify-between items-center'>
                <h1 className='text-2xl'>Cart</h1>
            </div>
            <AdminCartMain />
        </div>
    );
}
