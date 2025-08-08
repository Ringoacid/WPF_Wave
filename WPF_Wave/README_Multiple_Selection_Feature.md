# ListView�����I���@�\�ƃf�o�b�O�R�[�h�폜������

## �T�v
ListView�ŕ����̐M����I�����āu�g�`�ɒǉ��v�{�^���ňꊇ�ǉ�����@�\���������A�J�����Ɏg�p���Ă����f�o�b�O�p�̃{�^���ƃ��\�b�h��S�č폜���ăA�v���P�[�V�������N���[���A�b�v���܂����B

## ���������ύX���e

### 1. ListView�����I���@�\�̎���

#### MainWindow.xaml�ł̕ύX
```xaml
<!-- SelectionMode="Extended"�ŕ����I����L���� -->
<ListView
    Grid.Row="1"
    DisplayMemberPath="DisplayText"
    ItemsSource="{Binding ViewModel.SignalList}"
    SelectedItem="{Binding ViewModel.SelectedSignal}"
    SelectionMode="Extended"
    x:Name="SignalsListView"
    SelectionChanged="ListView_SelectionChanged" />
```

**��ȕύX�_:**
- `SelectionMode="Extended"`: Ctrl+�N���b�N�AShift+�N���b�N�ł̕����I����L��
- `SelectionChanged="ListView_SelectionChanged"`: �I��ύX���̃C�x���g�n���h���[��ǉ�
- `x:Name="SignalsListView"`: �R�[�h�r�n�C���h����A�N�Z�X�\�ɂ��閼�O��ݒ�

#### MainWindow.xaml.cs�ł̕ύX
```csharp
/// <summary>
/// ListView �̑I�����ڂ��ύX���ꂽ���̏���
/// �����I�����ꂽ�M����ViewModel�ɒʒm����
/// </summary>
private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (sender is ListView listView)
    {
        // �I�����ꂽ�M����ViewModel�ɐݒ�
        ViewModel.SelectedSignals = listView.SelectedItems.Cast<VariableDisplayItem>().ToList();
    }
}
```

**�@�\����:**
- ListView.SelectedItems���畡���I�����ꂽ���ڂ��擾
- LINQ �� Cast<T>() �� ToList() ��ViewModel�̃v���p�e�B�ɓK�����`�ɕϊ�
- ViewModel��SelectedSignals�v���p�e�B�ɐݒ�

### 2. ViewModel�ł̕����I��Ή�

#### �V�����v���p�e�B�̒ǉ�
```csharp
/// <summary>
/// ListView �Ō��ݑI������Ă��镡���̐M��
/// �u�g�`�ɒǉ��v�{�^���Ŕg�`�\�����X�g�ɒǉ�����Ώۂ̐M���Q
/// </summary>
[ObservableProperty]
List<VariableDisplayItem> selectedSignals = new();
```

#### AddSelectedSignalToWaveform�R�}���h�̊g��
```csharp
[RelayCommand]
public void AddSelectedSignalToWaveform()
{
    if (SelectedSignals != null && SelectedSignals.Any())
    {
        var addedCount = 0;
        var duplicateCount = 0;
        
        foreach (var selectedSignal in SelectedSignals)
        {
            // �d���`�F�b�N
            var existingSignal = SelectedSignalsForWaveform.OfType<VariableDisplayItem>().FirstOrDefault(s => 
                s.Name == selectedSignal.Name && 
                s.Type == selectedSignal.Type && 
                s.BitWidth == selectedSignal.BitWidth);

            if (existingSignal == null)
            {
                // �V�����C���X�^���X���쐬���Ēǉ�
                var newSignal = new VariableDisplayItem
                {
                    Name = selectedSignal.Name,
                    Type = selectedSignal.Type,
                    BitWidth = selectedSignal.BitWidth,
                    VariableData = selectedSignal.VariableData
                };
                SelectedSignalsForWaveform.Add(newSignal);
                addedCount++;
            }
            else
            {
                duplicateCount++;
            }
        }
        
        // ���ʂ��ڍׂɃ��[�U�[�ɒʒm
        if (addedCount > 0 && duplicateCount > 0)
        {
            MessageBox.Show($"{addedCount}�̐M����ǉ����܂����B\n{duplicateCount}�̐M���͊��ɒǉ��ς݂̂��߃X�L�b�v���܂����B", 
                "�ꊇ�ǉ�����", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (addedCount > 0)
        {
            MessageBox.Show($"{addedCount}�̐M����g�`�\�����X�g�ɒǉ����܂����B", 
                "�ǉ�����", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (duplicateCount > 0)
        {
            MessageBox.Show($"�I�����ꂽ{duplicateCount}�̐M���͑S�Ċ��ɔg�`�\�����X�g�ɒǉ�����Ă��܂��B", 
                "�d���G���[", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    else
    {
        MessageBox.Show("�ǉ�����M����I�����Ă��������B", 
            "�I���G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
```

**�@�\�������e:**
- **�ꊇ����**: �I�����ꂽ�S�Ă̐M������x�ɏ���
- **�d���h�~**: ���ɒǉ��ς݂̐M�����X�L�b�v
- **�ڍׂȌ��ʒʒm**: �ǉ����Əd�����𕪂��ĕ\��
- **�G���[�n���h�����O**: ���I�����̓K�؂ȃ��b�Z�[�W

### 3. �f�o�b�O�R�[�h�̊��S�폜

#### �폜����UI�v�f�iMainWindow.xaml�j
```xaml
<!-- �폜���ꂽ�e�X�g�{�^���Q -->
<StackPanel Grid.Row="0" Grid.Column="0" Orientation="Horizontal" Margin="5">
    <Button Command="{Binding ViewModel.ModifyFirstSignalCommand}" Content="First�ύX" Margin="2" Padding="5"/>
    <Button Command="{Binding ViewModel.AddTestSignalToWaveformCommand}" Content="�e�X�g�M���ǉ�" Margin="2" Padding="5"/>
    <Button Command="{Binding ViewModel.RemoveLastSignalFromWaveformCommand}" Content="�Ō�폜" Margin="2" Padding="5"/>
</StackPanel>
```

#### �폜����ViewModel�R�[�h
```csharp
// �폜���ꂽ�e�X�g�R�}���h
[RelayCommand] public void ModifyFirstSignal()
[RelayCommand] public void AddTestSignalToWaveform()
[RelayCommand] public void RemoveLastSignalFromWaveform()

// �폜���ꂽ�e�X�g�f�[�^������
public MainWindowViewModel()
{
    SelectedSignalsForWaveform.Add(new VariableDisplayItem {...});
    // ���̃e�X�g�f�[�^...
}
```

## ���������@�\�̏ڍ�

### 1. �����I���̃��[�U�[�G�N�X�y���G���X

#### �I����@
- **�P��I��**: �N���b�N��1�̐M����I��
- **�A���I��**: Shift+�N���b�N�Ŕ͈͑I��
- **�ʑI��**: Ctrl+�N���b�N�Ōʂɒǉ�/���O
- **�S�I��**: Ctrl+A �őS�Ă̐M����I��

#### ���o�I�t�B�[�h�o�b�N
- �I�����ꂽ���ڂ��n�C���C�g�\��
- �����I�����͑S�Ă̑I�����ڂ������Ƀn�C���C�g

### 2. �ꊇ�ǉ��̏����t���[

1. **�I���m�F**: 1�ȏ�̐M�����I������Ă��邩�`�F�b�N
2. **�d������**: �e�M���ɂ��Ċ������X�g�Ƃ̏d�����`�F�b�N
3. **�C���X�^���X�쐬**: �d�����Ă��Ȃ��M���̐V�����C���X�^���X���쐬
4. **�ǉ����s**: �g�`�\�����X�g�ɒǉ�
5. **���ʒʒm**: �ǉ����Əd�������ڍׂɕ�

### 3. �G���[�n���h�����O�̋���

#### �󋵕ʃ��b�Z�[�W
- **����**: �u���̐M����g�`�\�����X�g�ɒǉ����܂����v
- **��������**: �u���ǉ��A���͏d���̂��߃X�L�b�v�v
- **�S�d��**: �u�I�����ꂽ���̐M���͑S�Ċ��ɒǉ��ς݁v
- **���I��**: �u�ǉ�����M����I�����Ă��������v

### 4. �A�[�L�e�N�`���̉��P

#### MVVM�p�^�[���̓O��
- **View**: UI����i�����I���j�݂̂�S��
- **ViewModel**: �r�W�l�X���W�b�N�i�d���`�F�b�N�A�ǉ������j��S��
- **Model**: �f�[�^�\���iVariableDisplayItem�j�̒�`

#### �ێ琫�̌���
- �f�o�b�O�R�[�h�̍폜�ɂ��R�[�h�̊Ȍ���
- �{�ԋ@�\�ɏW�������݌v
- ���m�ȐӔC���S

## �g�p�V�i���I

### 1. �P��M���̒ǉ�
1. ListView �ŐM����1�N���b�N
2. �u�g�`�ɒǉ��v�{�^�����N���b�N
3. �I�������M���� DragableList �ɒǉ������

### 2. �����M���̈ꊇ�ǉ�
1. Ctrl+�N���b�N�ŕ����̐M����I��
2. �u�g�`�ɒǉ��v�{�^�����N���b�N
3. �I�������S�Ă̐M���� DragableList �Ɉꊇ�ǉ������

### 3. �͈͑I���ł̒ǉ�
1. �ŏ��̐M�����N���b�N
2. Shift+�N���b�N�ōŌ�̐M����I���i�͈͑I���j
3. �u�g�`�ɒǉ��v�{�^�����N���b�N
4. �I��͈͂̑S�Ă̐M�����ǉ������

## �p�t�H�[�}���X�l������

### 1. ��ʑI�����̏���
- foreach ���[�v�ɂ�鏇�������ň��萫���m��
- �d���`�F�b�N��LINQ�����͌����I�� FirstOrDefault ���g�p

### 2. UI������
- �ꊇ��������UI���ł܂�Ȃ��݌v
- ����������Ɍ��ʂ��܂Ƃ߂ĕ\��

### 3. ����������
- ���̐M���I�u�W�F�N�g�͎Q�Ƃ̂ݕێ�
- �\���p�̐V�����C���X�^���X���쐬���ēƗ������m��

## ����̊g���\��

### 1. �I����Ԃ̉i����
- �A�v���P�[�V�����ċN�����̑I����ԕ���
- �Z�b�V�����Ԃł̏�ԕێ�

### 2. ���x�ȑI���@�\
- �t�B���^�����O�@�\�Ƃ̘A�g
- �������ʂ���̈ꊇ�I��

### 3. �h���b�O&�h���b�v�Ή�
- ListView ���� DragableList �ւ̒��ڃh���b�O
- �����M���̓����h���b�O

���̎����ɂ��AVCD�g�`�r���[�A�̃��[�U�r���e�B���啝�Ɍ��サ�A�����I�ȐM���Ǘ����\�ɂȂ�܂����B�܂��A�f�o�b�O�R�[�h�̍폜�ɂ��A�v���_�N�V�����i���̃N���[���ȃR�[�h�x�[�X����������Ă��܂��B