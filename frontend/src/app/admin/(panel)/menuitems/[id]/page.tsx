import MenuItemInfo from "@/components/admin/menuItems/details/MenuItemInfo";

export default async function AdminMenuItemsDetailsPage ({ params }: { params: Promise<{ id: string }>}){
    const { id } = await params;

    return (
        <div className='flex flex-1 flex-col m-4 gap-4'>
            <MenuItemInfo itemId={id} />
        </div>
    );
}