import { Injectable } from '@angular/core';

export interface Menu {
  state: string;
  name: string;
  type: string;
  icon: string;
}

const MENUITEMS = [
  { state: 'users', type: 'link', name: 'Users', icon: 'person' },
  { state: 'roles', type: 'link', name: 'Roles', icon: 'group' },
];

@Injectable()
export class MenuItems {
  getMenuitem(): Menu[] {
    return MENUITEMS;
  }
}
