   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TektaRevitPluginsTektaRevitPlugins  5   	   /    )TektaRevitPlugins.ComboScheduleCreatorWin   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:TektaRevitPlugins     5   	     � Title$  Schedule Generator��5   	    � SizeToContent$ Height=�6$   $	��350��6;    � 
ResizeMode$ NoResize=�6G    � 	Resources 5	         myButton �� 
TargetType �� Property �� Value��%�  &	        '�� �     '�� �     '��     �� 5
   
   " ��6!   .�5      ��  � ��$ 3q�6'   �� 5       � ��$ 3q�6(   �� 5       � ��$ 60��6&   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6'   �� 5       �@��SelectedIndex$ 0��6.   �� 5       �@��
IsEditable .  6+   5      5      .��5      �  � RowDefinitions 5    
   �� 5!      $
��Auto �6   �� 5"      $
��Auto �6   �� 5#      $
��Auto �6    � ColumnDefinitions 5&   
   �� 5'      $��* �6   �� 5(      $
��Auto �6   .+�5+   
   �� $��
Properties��6   
���   �$��5q�6;   .��5,      � $��5q�6    5-      �� 5.      $��* �6$   �� 5/      $��* �6$   �� 50      $��* �6$   �� 51      $��* �6$   �� 52      $��* �6$    55      �� 56      $
��Auto �6'   �� 57      $��* �6'   .+�5:      �� $��0��6   $��0��6)   $)��#Schedule (Спецификация)��69   �� 5;      -   5        	 ��Name$	 cb_schedule_types��5;      $��0��66   $��1��6C   6S   �� 5<      $��1��6   $��0��6)   $��Partiton (Раздел)��69   �� 5=      -   5        $	 cb_partitions��5=      $��1��62   $��1��6?   6O   �� 5>      $��2��6   $��0��6)   $3��-Host Mark (Марка Конструкции)��69   $
��Wrap=�6^   �� 5?      -   5        $	 cb_host_marks��5?      $��2��62   $��1��6?   6O   �� 5@      $��3��6   $��0��6)   $-��'Assembly Mark (Марка Сборки)��69   $
��Wrap=�6]   �� 5A      -   5        $	 cb_assembly_marks��5A      $��3��66   $��1��6C   5B      5C      �� 5E   
   $��2��6   $��2��6"   
 �� Orientation$
 
Horizontal=�66   $��Right=�6O   .+�5F      �� -   5         ��Name$ btn_ok��5F      #��[  6%   $��Ok��6G   6T   �� 5G      -   5        $ 
btn_cancel��5G      #��[  6)   $��Cancel��6K   6\   5H      5J      5K      