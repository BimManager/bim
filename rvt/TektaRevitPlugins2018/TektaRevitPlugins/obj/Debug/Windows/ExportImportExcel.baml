   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TektaRevitPluginsTektaRevitPlugins  5   	   )    #TektaRevitPlugins.ExportImportExcel   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:TektaRevitPlugins     5   	     � Title$  ExportImportExcel��5   	   $	��500��6#   $	��400��60    � 	Resources 5	         btns �� 
TargetType �� Property �� Value  rad_btns��%!  &	        &	 p     �� 5
   
   " ��6   .�5      ��  � ��$ 0,5,5,5q�6'   �� 5       � ��$ 3q�6(   5      �� 5   
   " 	�6!   .�5      ��  � ��$ 0,3,0,0q�6'   �� 5       � ��$ Center=�62   �� 5       � ��$ Center=�69   5      5      .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��5,5,5,0q�6"   .@�5      �� $��Export��6   .��5      � -   5         �Name$ groupBox_grid��5       5      �� 5      $��* �6(   �� 5      $
��Auto �6(   .+�5!      �� $��0��6   $��	Schedules��6,   .��5"      � -   5        $ schedules_grid��5"      $��3,3,3,0q�67    5#      �� 5$   "   $��* �60   �� 5%   "   $
��Auto �60    � ColumnDefinitions 5'      �� 5(   "   $
��Auto �63   �� 5)   "   $��* �63   .+�5+      �� -   5         ��Name$ 	schedules��5+   &   $��0��69   $��2��6F   ��6Z   �� ��5,   "   �� 5-   &   .R�5.   *   �� 	 ��Name$	 checkBox_schedules��63   
 d� 	IsChecked$
 FalseY�5/   U   {�. ��67   ��  �� Path$
 Name�6g   50   '   52      �� 53      $��1��6)   $��0��66    �� Orientation$ 
Horizontal=�6F   .+�54   "   �� -   5         ��Name$ checkall_btn��54   )   #��[  6?   $��	Check All��6]   6q   �� 55   "   -   5        $ checknone_btn��55   )   #��[  6@   $��
Check None��6^   6s   56      �� 57      $��1��6'   $��1��64   $��0q�6D   $��0,2,0,0q�6X   .��58   "   �� .+�59   &   	� -   5         	�Name$ oneFile_rbtn��59   2   #��[ 6H   $$��One file, one tab per schedule��5:   2   $

 TrueY�6[   {�. 6l   	� 5;   &   -   5        $ manyFiles_rbtn��5;   2   #��[ 6J   $��One file per schedule��5<   2   {�. 6R   5=   #   5>      5?      5@      �� 5A      $��1��6   $��Export Options��6,   .��5B      � -   5        $ export_grid��5B      $��3q�64    5C      �� 5D   "   $
��Auto �63   �� 5E   "   $��* �63   �� 5F   "   $
��Auto �63   .+�5I      �� $��0��6(   $��Output folder:��68   $��2q�6N    �� Padding$ 0,2,0,2q�6Y   �� 5J      -	   5         ��Name$ filepath��5J   &   $��1��68   $��Stretch=�6H   $��2q�6f    �� 
IsReadOnly . 6q   �� 5K      -
   5        $ 
browse_btn��5K   5   $��2��6%   $��Browse��6b   $��2q�6s   $��2q�6~   5L      5M      5N      5O      5P      �� 5R   
   $��1��6   $ 
Horizontal=�6"   $��Right=�6;   $��5q�6W   .+�5S      �� -   5        $ 
export_btn��5S      $��Export��6)   $��0,0,5,0q�5T      $��5q�6&   �� 5U      -   5        $ 
cancel_btn��5U      $��Cancel��6)   $��5,0,0,0q�5V      $��5q�6&   5W      5Y      5Z      