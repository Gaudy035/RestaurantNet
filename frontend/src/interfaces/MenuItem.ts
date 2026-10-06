interface MenuItem {
    itemId: string;
    categoryId: string;
    name: string;
    price: number;
    imageUrl: string | null;
    isAvailable: boolean;
    isPinned: boolean;
}

export default MenuItem;