   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:TektaRevitPlugins.WindowsTektaRevitPlugins.Windows  5   	   +    %TektaRevitPlugins.Windows.CpValuesWnd   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   9local'clr-namespace:TektaRevitPlugins.Windows     5   	     � Title$  Copy Parameter Values��5   	   $	��300��6'   $	��400��64    � 
ResizeMode$ NoResize=�5	   	   #��Z�6    � 	Resources 5
       �� 
TargetType �� Property �� Value��%�  '��       '�� G     '�� �     '��     �� 5   
   " ��6   .�5      ��  � ��$ 5q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 5q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 75��6&   �� 5       � ��$ 22��6'   �� 5       � ��$ 5q�6'   5      �� 5   
   " ��6   .�5      ��  � ��# Z��6+   5      5      .��5      � $��5q�6   #��Z�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6    � ColumnDefinitions 5!   
   �� 5"      $��* �6   �� 5#      $��* �6   .+�5&   
   �� $��0��6   $��0��6!   $��	Copy From��61   $��Center=�6B   �� 5'   
   $��0��6   $��1��6!   $��Copy To��61   $��Center=�6@   �� 5)   
   -   5         ��Name$ lb_copy_from��5)      $��1��6(   $��0��65   5*      �� 5,   
   -   5        $ 
lb_copy_to��5,      $��1��6&   $��2��63   �� 5.   
   $��2��6   $��0��6"   .+�5/      �� -   5        	 ��Name$	 	txt_value��5/      $��Center=�50      
 �� 
IsReadOnly
 . 6/   51      �� 53   
   $��2��6   $��1��6"    �� Orientation$ 
Horizontal=�54      $��Center=�6.   .+�55      �� -   5         ��Name$ btn_ok��55      $��OK��6%   62   �� 56      -   5        $ 
btn_cancel��56      $��Cancel��6)   6:   57      58      59      