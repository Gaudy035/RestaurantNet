'use client';

import { Moon, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { Button } from './button';

export default function ModeToggle() {
  const { theme, setTheme } = useTheme();

  return (
    <Button
      size='icon'
      onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
    >
      <Sun className='scale-0 dark:scale-100 absolute'></Sun>
      <Moon className='dark:scale-0 absolute'></Moon>
    </Button>
  );
}
