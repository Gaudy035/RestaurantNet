import MenuItemUpdateForm from "@/components/admin/menuItems/details/MenuItemUpdateForm";

export default async function AdminMenuItemUpdatePage({ params }: { params:Promise<{ id: string }> }){
    const { id } = await params;

    return (
        <div className='flex flex-1 justify-center items-center'>
            <div className='w-full max-w-sm'>
                <MenuItemUpdateForm itemId={id} />
            </div>
        </div>
    );
}